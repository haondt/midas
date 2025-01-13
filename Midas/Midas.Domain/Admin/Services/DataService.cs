using Haondt.Core.Models;
using Microsoft.Extensions.Options;
using Midas.Core.Constants;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Admin.Models;
using Midas.Domain.Kvs.Models;
using Midas.Domain.Kvs.Services;
using Midas.Domain.NodeRed.Services;
using Midas.Domain.Shared.Models;
using Midas.Domain.Shared.Services;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;
using Newtonsoft.Json;

namespace Midas.Domain.Admin.Services
{
    public class DataService(
        IAsyncJobRegistry jobRegistry,
        INodeRedService nodeRedService,
        IOptions<TakeoutSettings> options,
        IFileService fileService,
        IAccountsService accountsService,
        ITransactionImportDataStorage importDataStorage,
        IKvsService kvsService,
        ITransactionService transactionService,
        ITransactionImportConfigurationStorage importConfigurationStorage,
        IDataExportStorage dataExportStorage) : IDataService
    {
        public Task<int> DeleteAllTransactions()
        {
            return transactionService.DeleteAllTransactions();
        }

        public Task<int> DeleteAllAccounts()
        {
            return accountsService.DeleteAllAccounts();
        }

        public Task<int> DeleteAllMappings()
        {
            return kvsService.DeleteAllMappings();
        }

        private async Task AddKvsMappingsToTakeout(string workDir)
        {
            var version = 1;
            var mappings = (await kvsService.ExportMappings()).Mappings
                .ToDictionary(q => q.Key, q => new TakeoutKvsMappingDto
                {
                    Aliases = q.Value.Aliases,
                    Value = q.Value.Value
                });

            var fileData = JsonConvert.SerializeObject(new TakeoutKvsMappingsDto
            {
                Mappings = mappings,
                Version = version
            }, MidasConstants.PrettySerializerSettings);
            await fileService.CreateTakeoutFileAsync(workDir, "kvs.json", fileData);
        }

        private async Task AddAccountsToTakeout(string workDir)
        {
            var version = 1;

            var page = 1;
            var currentAccountsData = await accountsService.GetPagedAccounts(options.Value.DatabaseOperationBatchSize, page);
            Dictionary<string, TakeoutAccountDto> accounts = [];
            while (currentAccountsData.Count > 0)
            {
                foreach (var (k, v) in currentAccountsData)
                    accounts[k] = new TakeoutAccountDto
                    {
                        IsMine = v.IsMine,
                        Name = v.Name
                    };
                page++;
                currentAccountsData = await accountsService.GetPagedAccounts(options.Value.DatabaseOperationBatchSize, page);
            }
            var accountsDataString = JsonConvert.SerializeObject(new TakeoutAccountsDto
            {
                Accounts = accounts,
                Version = version
            }, MidasConstants.PrettySerializerSettings);
            await fileService.CreateTakeoutFileAsync(workDir, "accounts.json", accountsDataString);
        }
        private async Task AddTransactionsToTakeout(string workDir)
        {
            var version = 1;

            var page = 1;
            var currentTransactions = await transactionService.GetPagedTransactions([], options.Value.DatabaseOperationBatchSize, page);
            Dictionary<long, TakeoutTransactionDto> transactionsData = [];
            while (currentTransactions.Count > 0)
            {
                foreach (var (k, v) in currentTransactions)
                    transactionsData[k] = new TakeoutTransactionDto
                    {
                        Amount = v.Amount,
                        DestinationAccount = v.DestinationAccount,
                        SourceAccount = v.SourceAccount,
                        Category = v.Category,
                        Description = v.Description,
                        Tags = v.Tags,
                        TimeStamp = v.TimeStamp,
                    };

                var transactionIds = transactionsData.Keys.ToList();
                foreach (var (k, v) in transactionIds.Zip(await importDataStorage.GetMany(transactionIds)))
                    transactionsData[k].ImportData = v.Select(q => new TakeoutTransactionImportDataDto
                    {
                        Account = q.Account,
                        ConfigurationSlug = q.ConfigurationSlug,
                        SourceData = q.SourceData
                    }).ToList();

                page++;
                currentTransactions = await transactionService.GetPagedTransactions([], options.Value.DatabaseOperationBatchSize, page);
            }
            var transactionDataString = JsonConvert.SerializeObject(new TakeoutTransactionsDto
            {
                Transactions = transactionsData.Select(kvp => kvp.Value).ToList(),
                Version = version,
            }, MidasConstants.PrettySerializerSettings);
            await fileService.CreateTakeoutFileAsync(workDir, "transactions.json", transactionDataString);
        }

        private async Task AddImportConfigurationsToTakeout(string workDir)
        {
            var version = 1;
            var configurations = await importConfigurationStorage.GetAll();
            var configurationsDataString = JsonConvert.SerializeObject(new TakeoutImportConfigurationsDto { Version = version, Slugs = configurations }, MidasConstants.PrettySerializerSettings);
            await fileService.CreateTakeoutFileAsync(workDir, "importConfigurations.json", configurationsDataString);
        }

        public string StartCreateTakeout()
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {
                var result = new TakeoutResult();
                var workDir = fileService.CreateTakeoutWorkDirectory(jobId);

                try
                {
                    var targetDbPath = fileService.GetTakeoutFilePath(workDir, "midas.db");
                    dataExportStorage.Export(targetDbPath);

                    var nodeRedData = await nodeRedService.ExportDataAsync();
                    await fileService.CreateTakeoutFileAsync(workDir, "flows.json", nodeRedData.Flows);
                    await fileService.CreateTakeoutFileAsync(workDir, "settings.js", nodeRedData.Settings);

                    await AddKvsMappingsToTakeout(workDir);
                    await AddAccountsToTakeout(workDir);
                    await AddTransactionsToTakeout(workDir);
                    await AddImportConfigurationsToTakeout(workDir);

                    var zipPath = fileService.ZipTakeoutDirectory(workDir);
                    result.ZipPath = zipPath;
                }
                catch (Exception ex)
                {
                    result.Errors = [ex.ToString()];
                    result.IsSuccessful = false;
                    jobRegistry.FailJob(jobId, result);
                    throw;
                }

                jobRegistry.CompleteJob(jobId, result);
            });

            return jobId;
        }

        public Result<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId)
        {
            var (status, progress, message) = jobRegistry.GetJobProgress(jobId);
            if (status < AsyncJobStatus.Complete)
                return new(message);
            var result = jobRegistry.GetJobResult(jobId);
            if (!result.HasValue)
                throw new InvalidOperationException($"Job {jobId} has status {status} and no result.");
            if (result.Value is not TakeoutResult castedResult)
                throw new InvalidOperationException($"Job {jobId} has status {status} and a result of type {result.Value.GetType()} instead of {typeof(TakeoutResult)}.");
            return new(castedResult);
        }

        public Task ImportKvsMappings(TakeoutKvsMappingsDto mappings, bool overwriteExisting)
        {
            if (mappings.Version != 1)
                throw new InvalidOperationException($"Unable to import version {mappings.Version}");

            return kvsService.ImportKvsMappings(new ExternalKvsMappingsDto
            {
                Mappings = mappings.Mappings.ToDictionary(q => q.Key, q => new ExternalKvsMappingDto
                {
                    Aliases = q.Value.Aliases,
                    Value = q.Value.Value
                })

            }, overwriteExisting);
        }

        public Task ImportAccounts(TakeoutAccountsDto accounts, bool overwriteExisting)
        {
            if (accounts.Version != 1)
                throw new InvalidOperationException($"Unable to import version {accounts.Version}");

            var accountDtos = accounts.Accounts.Select(kvp => (kvp.Key, new AccountDto
            {
                IsMine = kvp.Value.IsMine,
                Name = kvp.Value.Name,
            })).ToList();

            if (overwriteExisting)
                return accountsService.UpsertAccounts(accountDtos);
            return accountsService.CreateAccounts(accountDtos);
        }

        public async Task ImportTransactions(TakeoutTransactionsDto transactions)
        {
            if (transactions.Version != 1)
                throw new InvalidOperationException($"Unable to import version {transactions.Version}");

            var transactionDtos = transactions.Transactions.Select(t => new TransactionDto
            {
                Amount = t.Amount,
                DestinationAccount = t.DestinationAccount,
                SourceAccount = t.SourceAccount,
                Category = t.Category,
                Description = t.Description,
                Tags = t.Tags,
                TimeStamp = t.TimeStamp
            }).ToList();


            var transactionIds = await transactionService.CreateTransactions(transactionDtos);
            await importDataStorage.AddMany(transactions.Transactions
                .Zip(transactionIds)
                .SelectMany(q => q.First.ImportData.Select(r => new TransactionImportDataDto
                {
                    Account = r.Account,
                    ConfigurationSlug = r.ConfigurationSlug,
                    SourceData = r.SourceData,
                    Transaction = q.Second
                })));
        }

        public Task ImportTransactionImportConfigurations(TakeoutImportConfigurationsDto configurations)
        {
            if (configurations.Version != 1)
                throw new InvalidOperationException($"Unable to import version {configurations.Version}");
            return importConfigurationStorage.AddMany(configurations.Slugs);
        }
    }
}
