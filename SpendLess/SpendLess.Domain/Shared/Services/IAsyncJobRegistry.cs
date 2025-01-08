
using Haondt.Core.Models;
using SpendLess.Domain.Shared.Models;

namespace SpendLess.Domain.Shared.Services
{
    public interface IAsyncJobRegistry
    {
        void CancelJob(string jobId);
        void CompleteJob(string jobId, object result);
        void FailJob(string jobId, object? result = null, bool? requestCancellation = null);
        AsyncJobStatus GetJobStatus(string jobId);
        (AsyncJobStatus Status, double Progress, Optional<string> ProgressMessage) GetJobProgress(string jobId);
        void UpdateJobProgress(string jobId, double progress, Optional<Optional<string>> progressMessage = default);
        (string, CancellationToken) RegisterJob(Optional<string> progressMessage = default);
        Optional<object> GetJobResult(string jobId);
        Result<T, (double Progress, Optional<string> Message)> GetJobResultOrProgress<T>(string jobId);
    }
}
