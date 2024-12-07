using Haondt.Core.Models;
using Microsoft.Extensions.Logging;

namespace SpendLess.Domain.Services
{
    public class AsyncJobRegistry(ILogger<AsyncJobRegistry> logger) : IAsyncJobRegistry
    {
        private Dictionary<string, AsyncJob> _jobs = [];
        private readonly TimeSpan _expiration = TimeSpan.FromHours(1);

        public (string, CancellationToken) RegisterJob(Optional<string> progressMessage = default)
        {
            var jobId = Guid.NewGuid().ToString();
            var job = new AsyncJob
            {
                Timer = new System.Timers.Timer(_expiration.TotalMilliseconds)
                {
                    AutoReset = false,
                },
                ProgressMessage = progressMessage
            };
            job.Timer.Elapsed += (_, _) =>
            {
                try
                {
                    if (job.Status < AsyncJobStatus.Complete)
                    {
                        job.Status = AsyncJobStatus.TimedOut;
                        job.CancellationTokenSource.Cancel();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to time out job {jobId}.");
                }
                finally
                {
                    _jobs.Remove(jobId);
                }
            };
            _jobs[jobId] = job;
            job.Timer.Enabled = true;

            return (jobId, job.CancellationTokenSource.Token);
        }

        public Optional<object> GetJobResult(string jobId)
        {
            var job = GetJob(jobId);
            if (job.Status < AsyncJobStatus.Complete)
                throw new InvalidOperationException($"Job {jobId} is still executing.");
            return job.Result;
        }

        public void CompleteJob(string jobId, object result)
        {
            var job = GetJob(jobId);
            if (job.Status >= AsyncJobStatus.Complete)
                return;

            job.Status = AsyncJobStatus.Complete;
            job.Result = result;
        }

        public void CancelJob(string jobId)
        {
            var job = GetJob(jobId);
            if (job.Status >= AsyncJobStatus.Complete)
                return;
            job.CancellationTokenSource.Cancel();
            job.Status = AsyncJobStatus.Aborted;
        }

        public void FailJob(string jobId, object? result = null, bool? requestCancellation = null)
        {
            var job = GetJob(jobId);
            if (job.Status >= AsyncJobStatus.Complete)
                return;
            job.Status = AsyncJobStatus.Failed;
            if (result != null)
                job.Result = new(result);
            if (requestCancellation.HasValue && requestCancellation.Value == true)
                job.CancellationTokenSource.Cancel();
        }

        public AsyncJobStatus GetJobStatus(string jobId)
        {
            var job = GetJob(jobId);
            return job.Status;
        }

        private AsyncJob GetJob(string jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job) || job is null)
                throw new KeyNotFoundException($"No job with id {jobId}");
            return job;
        }

        public (AsyncJobStatus, double, Optional<string>) GetJobProgress(string jobId)
        {
            var job = GetJob(jobId);
            return (job.Status, job.Progress, job.ProgressMessage);
        }

        public void UpdateJobProgress(string jobId, double progress, Optional<Optional<string>> progressMessage = default)
        {
            var job = GetJob(jobId);
            job.Progress = progress;
            if (progressMessage.HasValue)
                job.ProgressMessage = progressMessage.Value;
        }
    }
}
