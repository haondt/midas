using Haondt.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Admin.Models;
using Midas.Domain.NodeRed.Services;
using Midas.Domain.Shared.Models;
using Midas.Domain.Shared.Services;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Admin.Services
{
    public class DataService(
        IAsyncJobRegistry jobRegistry,
        INodeRedService nodeRedService,
        IFileService fileService,
        IAccountsService accountsService,
        ITransactionService transactionService,
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

        public string StartCreateTakeout()
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {
                var result = new TakeoutResult();
                var workDir = fileService.CreateTakeoutWorkDirectory(jobId);

                try
                {
                    var targetDbPath = fileService.GetTakeoutFilePath(workDir, "spendless.db");
                    dataExportStorage.Export(targetDbPath);

                    var nodeRedData = await nodeRedService.ExportDataAsync();
                    await fileService.CreateTakeoutFileAsync(workDir, "flows.json", nodeRedData.Flows);
                    await fileService.CreateTakeoutFileAsync(workDir, "settings.js", nodeRedData.Settings);

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
    }
}
