using Haondt.Core.Models;
using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Services;
using SpendLess.Kvs.Services;

namespace SpendLess.Admin.Services
{
    public class DataService(IAsyncJobRegistry jobRegistry, IFileService fileService, IKvsService kvsService) : IDataService
    {
        public string StartCreateTakeout(
            bool includeMappings,
            bool includeAccounts,
            bool includeTransactions,
            bool includeFlows)
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {
                var result = new TakeoutResult();
                var workDir = fileService.CreateTakeoutWorkDirectory(jobId);
                var tasks = new List<Task>();

                if (includeMappings)
                    tasks.Add(Task.Run(async () =>
                    {
                        var mappings = await kvsService.ExportMappings();
                        var mappingsString = JsonConvert.SerializeObject(mappings, SpendLessConstants.PrettySerializerSettings);
                        await fileService.CreateTakeoutFile(workDir, "mappings.json", mappingsString);
                    }));

                try
                {
                    await Task.WhenAll(tasks);
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
