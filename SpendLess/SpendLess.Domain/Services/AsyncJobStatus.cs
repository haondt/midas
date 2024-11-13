namespace SpendLess.Domain.Services
{
    public enum AsyncJobStatus
    {
        Started,
        Complete,
        Aborted,
        TimedOut,
        Failed,
    }
}
