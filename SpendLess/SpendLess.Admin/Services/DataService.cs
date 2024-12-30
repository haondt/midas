using Haondt.Core.Models;
using SpendLess.Domain.Services;

namespace SpendLess.Admin.Services
{
    public class DataService(IAsyncJobRegistry jobRegistry) : IDataService
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
