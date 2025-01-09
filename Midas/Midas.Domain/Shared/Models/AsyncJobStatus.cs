namespace Midas.Domain.Shared.Models
{
    public enum AsyncJobStatus
    {
        Started,

        // beyond here the job is finished
        Complete,

        // beyond here the job may or may not have a result
        Aborted,
        TimedOut,
        Failed,
    }
}
