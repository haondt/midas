
namespace SpendLess.Domain.Services
{
    public interface IAsyncJobRegistry
    {
        void CancelJob(Guid jobId);
        void CompleteJob(Guid jobId, object result);
        void FailJob(Guid jobId, string? message = null, bool? requestCancellation = null);
        AsyncJobStatus GetJobStatus(Guid jobId);
        (Guid, CancellationToken) RegisterJob();
    }
}
