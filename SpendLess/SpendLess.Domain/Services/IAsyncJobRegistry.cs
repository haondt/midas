
using Haondt.Core.Models;

namespace SpendLess.Domain.Services
{
    public interface IAsyncJobRegistry
    {
        void CancelJob(string jobId);
        void CompleteJob(string jobId, object result);
        void FailJob(string jobId, object? result = null, bool? requestCancellation = null);
        AsyncJobStatus GetJobStatus(string jobId);
        (AsyncJobStatus, double) GetJobProgress(string jobId);
        void UpdateJobProgress(string jobId, double progress);
        (string, CancellationToken) RegisterJob();
        Optional<object> GetJobResult(string jobId);
    }
}
