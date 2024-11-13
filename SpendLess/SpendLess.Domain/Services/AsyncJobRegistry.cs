using Microsoft.Extensions.Logging;

namespace SpendLess.Domain.Services
{
    public class AsyncJobRegistry(ILogger<AsyncJobRegistry> logger) : IAsyncJobRegistry
    {
        private Dictionary<Guid, AsyncJob> _jobs = [];
        private readonly TimeSpan _expiration = TimeSpan.FromHours(1);

        public (Guid, CancellationToken) RegisterJob()
        {
            var jobId = Guid.NewGuid();
            var job = new AsyncJob
            {
                Timer = new System.Timers.Timer(_expiration.TotalSeconds)
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

        public void CompleteJob(Guid jobId, object result)
        {
            var job = GetJob(jobId);
            if (job.Status >= AsyncJobStatus.Complete)
                return;

            job.Status = AsyncJobStatus.Complete;
        }

        public void CancelJob(Guid jobId)
        {
            var job = GetJob(jobId);
            if (job.Status >= AsyncJobStatus.Complete)
                return;
            job.CancellationTokenSource.Cancel();
            job.Status = AsyncJobStatus.Aborted;
        }

        public void FailJob(Guid jobId, string? message = null, bool? requestCancellation = null)
        {
            var job = GetJob(jobId);
            if (job.Status >= AsyncJobStatus.Complete)
                return;
            job.Status = AsyncJobStatus.Failed;
            if (!string.IsNullOrEmpty(message))
                job.Result = new(message);
            if (requestCancellation.HasValue && requestCancellation.Value == true)
                job.CancellationTokenSource.Cancel();
        }

        public AsyncJobStatus GetJobStatus(Guid jobId)
        {
            var job = GetJob(jobId);
            return job.Status;
        }

        private AsyncJob GetJob(Guid jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job) || job is null)
                throw new KeyNotFoundException($"No job with id {jobId}");
            return job;
        }
    }
}
