using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Services
{
    public class AsyncJob
    {
        public static StorageKey<AsyncJob> GetStorageKey(Guid id) => StorageKey<AsyncJob>.Create(id.ToString());
        public AsyncJobStatus Status { get; set; } = AsyncJobStatus.Started;
        public Optional<object> Result { get; set; } = new();
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        public required System.Timers.Timer Timer { get; set; }
    }
}
