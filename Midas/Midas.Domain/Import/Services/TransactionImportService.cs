using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Microsoft.Extensions.Options;
using Midas.Core.Constants;
using Midas.Core.Extensions;
using Midas.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Import.Exceptions;
using Midas.Domain.Import.Models;
using Midas.Domain.NodeRed.Models;
using Midas.Domain.NodeRed.Services;
using Midas.Domain.Shared.Services;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Import.Services
{
    public class TransactionImportService(IAsyncJobRegistry jobRegistry,
        INodeRedService nodeRed,
        IOptions<TransactionImportSettings> options,
        IAccountsService accountsService,
        ITransactionImportDataStorage importDataStorage,
        ITransactionService transactionService) : ITransactionImportService
    {



        public Result<DryRunResultDto, (double, Optional<string>)> GetDryRunResult(string jobId)
        {
            return jobRegistry.GetJobResultOrProgress<DryRunResultDto>(jobId);
        }

        public Result<TransactionImportResultDto, (double, Optional<string>)> GetImportResult(string jobId)
        {
            return jobRegistry.GetJobResultOrProgress<TransactionImportResultDto>(jobId);
        }

        public string StartImport(string dryRunId)
        {
            var result = jobRegistry.GetJobResult(dryRunId);
            if (!result.HasValue || result.Value is not DryRunResultDto dryRunResult)
                throw new InvalidOperationException($"Job {dryRunId} has a result of type {result.Value.GetType()} instead of {typeof(DryRunResultDto)}.");

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {
                var result = new TransactionImportResultDto
                {
                    TotalTransactions = 0,
                    ImportTag = dryRunResult.ImportTag
                };
                try
                {
                    foreach (var newAccount in dryRunResult.NewAccounts)
                        await accountsService.CreateAccount(newAccount.Key, new AccountDto
                        {
                            Name = newAccount.Value,
                            IsMine = false
                        });

                    var batchSize = options.Value.StorageOperationBatchSize;
                    var batches = dryRunResult.Transactions.Chunk(batchSize);
                    foreach (var batch in batches)
                    {
                        var transactions = batch
                            .Where(t => t.TransactionData.HasValue)
                            .Select(t => new TransactionDto
                            {
                                Amount = t.TransactionData.Value.Amount,
                                Tags = t.TransactionData.Value.Tags,
                                Category = t.TransactionData.Value.Category,
                                SourceAccount = t.TransactionData.Value.Source.Id,
                                DestinationAccount = t.TransactionData.Value.Destination.Id,
                                Description = t.TransactionData.Value.Description,
                                TimeStamp = t.TransactionData.Value.TimeStamp,
                            })
                            .ToList();

                        var deleteTransactions = batch
                            .Where(q => q.ReplacementTarget.HasValue)
                            .Select(q => q.ReplacementTarget.Value)
                            .Distinct()
                            .ToList();
                        var transactionIds = await transactionService.ReplaceTransactions(transactions, deleteTransactions);
                        await importDataStorage.AddMany(transactionIds.Zip(batch).Select(q => new TransactionImportDataDto
                        {
                            Account = q.Second.ImportData.Account,
                            ConfigurationSlug = q.Second.ImportData.ConfigurationSlug,
                            SourceData = q.Second.ImportData.SourceData,
                            Transaction = q.First
                        }));

                        result.TotalTransactions += batch.Length;
                        jobRegistry.UpdateJobProgress(jobId, result.TotalTransactions / dryRunResult.Transactions.Count);
                    }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (Exception ex)
                {
                    result.Errors = [ex.ToString()];
                    result.IsSuccessful = false;
                    jobRegistry.FailJob(jobId, result);
                }
            });

            return jobId;
        }

        public string StartDryRun(
            List<TransactionFilter> filters,
            bool addImportTag,
            string? configurationSlug = null,
            string? accountId = null)
        {

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            var currentTimeStamp = AbsoluteDateTime.Now;
            var importTag = $"import-{currentTimeStamp}";

            _ = Task.Run(async () =>
            {
                var result = new DryRunResultDto();
                try
                {
                    if (addImportTag)
                        result.ImportTag = importTag;

                    DryRunAccountDto? globalImportAccount = null;

                    if (!string.IsNullOrEmpty(accountId))
                        globalImportAccount = new DryRunAccountDto
                        {
                            Id = accountId,
                            Name = (await accountsService.TryGetAccount(accountId))
                                .As(a => a.Name)
                                .Or(MidasConstants.FallbackAccountName)
                        };

                    var results = new List<(SendToNodeRedRequestDto Request, SendToNodeRedResponseDto Response)>();
                    var existingCategories = (await transactionService.GetCategories()).ToHashSet();
                    var existingTags = (await transactionService.GetTags()).ToHashSet();
                    var pageSize = Math.Min(options.Value.NodeRedOperationBatchSize, options.Value.StorageOperationBatchSize);
                    var page = 1;
                    var transactions = (await transactionService.GetPagedTransactions(filters, pageSize, page)).ToList();
                    var totalTransactions = await transactionService.GetTransactionsCount(filters);
                    var processed = 0;
                    while (transactions.Count > 0)
                    {
                        List<List<TransactionImportDataDto>> importDatas = [];
                        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(configurationSlug))
                            importDatas = await importDataStorage.GetMany(transactions.Select(t => t.Key));

                        var payloads = new List<(SendToNodeRedRequestDto Request, long TransactionId, bool WasMerged)>();
                        for (int i = 0; i < transactions.Count; i++)
                        {
                            var (tid, transaction) = transactions[i];
                            var datas = importDatas[i];
                            payloads.AddRange(datas.Select(d => (new SendToNodeRedRequestDto
                            {
                                Account = accountId ?? d.Account,
                                Configuration = configurationSlug ?? d.ConfigurationSlug,
                                Data = d.SourceData
                            }, tid, datas.Count > 1)));
                        }

                        var batchResults = await Task.WhenAll(payloads.Select(async p =>
                        {
                            try
                            {
                                var response = await nodeRed.SendToNodeRed(p.Request, cancellationToken);
                                return response.As(r => (p, r));
                            }
                            catch (Exception ex)
                            {
                                throw new SendToNodeRedException($"{ex.GetType()}: {ex.Message}", ex)
                                {
                                    Request = p.Request,
                                };
                            }
                        }));

                        foreach (var (source, response) in batchResults.Where(q => q.HasValue).Select(q => q.Value))
                        {
                            var context = new TransactionImportDryRunContext
                            {
                                Result = result,
                                CurrentTimeStamp = currentTimeStamp,
                                ExistingCategories = existingCategories,
                                ExistingTags = existingTags,
                            };

                            await ProcessNodeRedResult(source.Request, response, context);
                            context.Result.Transactions[^1].ReplacementTarget = source.TransactionId;
                            if (source.WasMerged)
                                context.Result.Transactions[^1].Warnings.Add(TransactionImportWarning.WasMerged);
                        }

                        foreach (var batch in result.Transactions.Chunk(options.Value.StorageOperationBatchSize))
                        {
                            var hasBeenImported = await importDataStorage.CheckIfHasSourceDataHash(batch.Select(r => TransactionImportDataDto.HashSourceData(r.SourceData)));
                            foreach (var resultDto in batch
                                .Zip(hasBeenImported))
                            {
                                if (resultDto.First.ReplacementTarget.HasValue)
                                    resultDto.First.Warnings.Add(TransactionImportWarning.WillUpdateExisting);
                                else if (resultDto.Second)
                                    resultDto.First.Warnings.Add(TransactionImportWarning.SourceDataHashExists);
                            }
                        }

                        processed += transactions.Count;
                        jobRegistry.UpdateJobProgress(jobId, processed / (double)totalTransactions);


                        page++;
                        transactions = (await transactionService.GetPagedTransactions(filters, pageSize, page)).ToList();
                    }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (SendToNodeRedException ex)
                {
                    result.Transactions = new List<DryRunTransactionDto>
                    {
                        new DryRunTransactionDto
                        {
                            SourceRequestPayload = ex.Request.ToString(),
                            SourceData = ex.Request.Data,
                            Errors = new HashSet<string>
                            {
                                ex.Message
                            },
                            ImportData = new DryRunTransactionImportData
                            {
                                Account = ex.Request.Account,
                                ConfigurationSlug = ex.Request.Configuration,
                                SourceData = ex.Request.Data,
                            }
                        }
                    };
                    jobRegistry.FailJob(jobId, result, true);
                    throw;
                }
                catch (Exception)
                {
                    jobRegistry.FailJob(jobId, requestCancellation: true);
                    throw;
                }
            });

            return jobId;
        }

        public string StartDryRun(
            List<List<string>> csvData,
            bool addImportTag,
            string configurationSlug,
            string accountId,
            TransactionImportConflictResolutionStrategy conflictResolutionStrategy)
        {

            var (jobId, cancellationToken) = jobRegistry.RegisterJob("parsing csv...");
            var currentTimeStamp = AbsoluteDateTime.Now;
            var importTag = $"import-{currentTimeStamp}";

            _ = Task.Run(async () =>
            {

                var result = new DryRunResultDto();
                try
                {
                    var importAccount = new DryRunAccountDto
                    {
                        Id = accountId,
                        Name = (await accountsService.TryGetAccount(accountId))
                            .As(a => a.Name)
                            .Or(MidasConstants.FallbackAccountName)
                    };


                    if (addImportTag)
                        result.ImportTag = importTag;

                    var results = new List<(SendToNodeRedRequestDto Request, SendToNodeRedResponseDto Response)>();

                    if (csvData.Count == 0)
                    {
                        jobRegistry.CompleteJob(jobId, result);
                        return;
                    }

                    var existingCategories = (await transactionService.GetCategories()).ToHashSet();
                    var existingTags = (await transactionService.GetTags()).ToHashSet();

                    var batchSize = options.Value.NodeRedOperationBatchSize;
                    var header = csvData.First();
                    var batches = csvData.Chunk(batchSize);

                    var processed = 0;
                    var isFirstBatch = true;
                    foreach (var batch in batches)
                    {
                        List<List<string>> filteredBatch = batch.ToList();
                        if (conflictResolutionStrategy == TransactionImportConflictResolutionStrategy.Omit)
                        {
                            var batchedHasBeenImported = await importDataStorage.CheckIfHasSourceDataHash(filteredBatch.Select(TransactionImportDataDto.HashSourceData));
                            filteredBatch = batch.Where((b, i) => !batchedHasBeenImported[i]).ToList();
                        }

                        var payloads = filteredBatch.Select(b => new SendToNodeRedRequestDto
                        {
                            Account = accountId,
                            Configuration = configurationSlug,
                            Data = b
                        });

                        if (isFirstBatch)
                        {
                            payloads = payloads.Take(1)
                                .Select(r => new SendToNodeRedRequestDto
                                {
                                    Account = r.Account,
                                    Configuration = r.Configuration,
                                    Data = r.Data,
                                    IsFirstRow = true
                                })
                                .Concat(payloads.Skip(1));
                        }

                        var batchResults = await Task.WhenAll(payloads.Select(async p =>
                        {
                            try
                            {
                                var response = await nodeRed.SendToNodeRed(p, cancellationToken);
                                return response.As(r => (p, r));
                            }
                            catch (Exception ex)
                            {
                                throw new SendToNodeRedException($"{ex.GetType()}: {ex.Message}", ex)
                                {
                                    Request = p
                                };
                            }
                        }));
                        results.AddRange(batchResults.Where(x => x.HasValue).Select(x => x.Value));
                        isFirstBatch = false;
                        processed += batch.Length;
                        jobRegistry.UpdateJobProgress(jobId, processed / (double)csvData.Count);
                    }

                    jobRegistry.UpdateJobProgress(jobId, processed / (double)csvData.Count);

                    var context = new TransactionImportDryRunContext
                    {
                        CurrentTimeStamp = currentTimeStamp,
                        ExistingCategories = existingCategories,
                        ExistingTags = existingTags,
                        Result = result
                    };
                    foreach (var (request, response) in results)
                        await ProcessNodeRedResult(request, response, context);

                    if (conflictResolutionStrategy == TransactionImportConflictResolutionStrategy.Warn)
                        foreach (var batch in result.Transactions.Chunk(options.Value.StorageOperationBatchSize))
                        {
                            var batchedHasBeenImported = await importDataStorage.CheckIfHasSourceDataHash(batch.Select(r => TransactionImportDataDto.HashSourceData(r.SourceData)));
                            foreach (var resultDto in batch
                                .Zip(batchedHasBeenImported)
                                .Where(zipped => zipped.Second)
                                .Select(zipped => zipped.First))
                                resultDto.Warnings.Add(TransactionImportWarning.SourceDataHashExists);
                        }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (SendToNodeRedException ex)
                {
                    result.Transactions = new List<DryRunTransactionDto>
                    {
                        new DryRunTransactionDto
                        {
                            SourceRequestPayload = ex.Request.ToString(),
                            SourceData = ex.Request.Data,
                            Errors = new HashSet<string>
                            {
                                ex.Message
                            },
                            ImportData = new DryRunTransactionImportData
                            {
                                Account = ex.Request.Account,
                                ConfigurationSlug = ex.Request.Configuration,
                                SourceData = ex.Request.Data
                            }
                        }
                    };
                    jobRegistry.FailJob(jobId, result, true);
                    throw;
                }
                catch (Exception)
                {
                    jobRegistry.FailJob(jobId, requestCancellation: true);
                    throw;
                }
            });

            return jobId;
        }

        public async Task ProcessNodeRedResult(SendToNodeRedRequestDto request, SendToNodeRedResponseDto response, TransactionImportDryRunContext context)
        {
            var resultDto = new DryRunTransactionDto
            {
                SourceRequestPayload = request.ToString(),
                SourceData = request.Data,
                ImportData = new()
                {
                    Account = request.Account,
                    ConfigurationSlug = request.Configuration,
                    SourceData = request.Data,
                },
            };

            // given an existing id, fetch the name
            if (response.Transaction.Source.Id != null &&
                (await accountsService.TryGetAccount(response.Transaction.Source.Id)).Test(out var account))
            {
                response.Transaction.Source.Name = account.Name;
            }
            // given a new id, either grab the name from the new accounts
            // or default it to the fallback name and add it to the new accounts
            else if (response.Transaction.Source.Id != null)
            {
                if (context.Result.NewAccounts.TryGetValue(response.Transaction.Source.Id, out var name))
                    response.Transaction.Source.Name = name;
                else
                    context.Result.NewAccounts[response.Transaction.Source.Id] = response.Transaction.Source.Name ??= MidasConstants.FallbackAccountName;
            }
            // given a name sans id, try and find the id based on the name in the new accounts
            // otherwise generate a new id and add it to the new accounts
            // also warn if we are going to create an account with the same name as an existing one
            else if (response.Transaction.Source.Name != null)
            {
                foreach (var (key, value) in context.Result.NewAccounts)
                    if (value == response.Transaction.Source.Name)
                    {
                        response.Transaction.Source.Id = key;
                        break;
                    }
                if (response.Transaction.Source.Id == null)
                {
                    response.Transaction.Source.Id = Guid.NewGuid().ToString();
                    context.Result.NewAccounts[response.Transaction.Source.Id] = response.Transaction.Source.Name;
                    if (await accountsService.HasAccountWithName(response.Transaction.Source.Name))
                        resultDto.Warnings.Add($"{TransactionImportWarning.CreatingAccountWithSameNameAsExisting} Name: {response.Transaction.Source.Name}.");
                }
            }
            // no id, no name, using defaults
            else
            {
                resultDto.Warnings.Add(TransactionImportWarning.MissingSourceAccount);
                response.Transaction.Source.Id = MidasConstants.DefaultAccount;
                response.Transaction.Source.Name = MidasConstants.DefaultAccountName;
            }

            // given an existing id, fetch the name
            if (response.Transaction.Destination.Id != null &&
                (await accountsService.TryGetAccount(response.Transaction.Destination.Id)).Test(out account))
            {
                response.Transaction.Destination.Name = account.Name;
            }
            // given a new id, either grab the name from the new accounts
            // or default it to the fallback name and add it to the new accounts
            else if (response.Transaction.Destination.Id != null)
            {
                if (context.Result.NewAccounts.TryGetValue(response.Transaction.Destination.Id, out var name))
                    response.Transaction.Destination.Name = name;
                else
                    context.Result.NewAccounts[response.Transaction.Destination.Id] = response.Transaction.Destination.Name ??= MidasConstants.FallbackAccountName;
            }
            // given a name sans id, try and find the id based on the name in the new accounts
            // otherwise generate a new id and add it to the new accounts
            // also warn if we are going to create an account with the same name as an existing one
            else if (response.Transaction.Destination.Name != null)
            {
                foreach (var (key, value) in context.Result.NewAccounts)
                    if (value == response.Transaction.Destination.Name)
                    {
                        response.Transaction.Destination.Id = key;
                        break;
                    }
                if (response.Transaction.Destination.Id == null)
                {
                    response.Transaction.Destination.Id = Guid.NewGuid().ToString();
                    context.Result.NewAccounts[response.Transaction.Destination.Id] = response.Transaction.Destination.Name;
                    if (await accountsService.HasAccountWithName(response.Transaction.Destination.Name))
                        resultDto.Warnings.Add($"{TransactionImportWarning.CreatingAccountWithSameNameAsExisting} Name: {response.Transaction.Destination.Name}.");
                }
            }
            // no id, no name, using defaults
            else
            {
                resultDto.Warnings.Add(TransactionImportWarning.MissingDestinationAccount);
                response.Transaction.Destination.Id = MidasConstants.DefaultAccount;
                response.Transaction.Destination.Name = MidasConstants.DefaultAccountName;
            }


            resultDto.TransactionData = new DryRunTransactionDataDto
            {
                Amount = response.Transaction.Amount,
                TimeStamp = response.Transaction.TimeStamp.HasValue ? response.Transaction.TimeStamp.Value : context.CurrentTimeStamp,
                Category = response.Transaction.Category,
                Tags = response.Transaction.Tags,
                Source = new DryRunAccountDto
                {
                    Id = response.Transaction.Source.Id,
                    Name = response.Transaction.Source.Name
                },
                Destination = new DryRunAccountDto
                {
                    Id = response.Transaction.Destination.Id,
                    Name = response.Transaction.Destination.Name
                },
                Description = response.Transaction.Description
            };

            if (context.Result.ImportTag.HasValue)
                resultDto.TransactionData.Value.Tags.Add(context.Result.ImportTag.Value);

            context.Result.Transactions.Add(resultDto);

            if (resultDto.TransactionData.Value.Source.Id == request.Account)
                context.Result.BalanceChange -= resultDto.TransactionData.Value.Amount;
            else if (resultDto.TransactionData.Value.Destination.Id == request.Account)
                context.Result.BalanceChange += resultDto.TransactionData.Value.Amount;

            foreach (var tag in resultDto.TransactionData.Value.Tags)
            {
                if (context.ExistingTags.Contains(tag))
                    continue;
                if (context.Result.NewTags.ContainsKey(tag))
                    context.Result.NewTags[tag] += 1;
                else
                    context.Result.NewTags[tag] = 1;
            }

            if (resultDto.TransactionData.Value.Category == MidasConstants.DefaultCategory)
                resultDto.Warnings.Add(TransactionImportWarning.MissingCategory);
            else if (!context.ExistingCategories.Contains(resultDto.TransactionData.Value.Category))
            {
                if (context.Result.NewCategories.ContainsKey(resultDto.TransactionData.Value.Category))
                    context.Result.NewCategories[resultDto.TransactionData.Value.Category] += 1;
                else
                    context.Result.NewCategories[resultDto.TransactionData.Value.Category] = 1;
            }
        }
    }
}
