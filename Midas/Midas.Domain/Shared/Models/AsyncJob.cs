using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace Midas.Domain.Shared.Models
{
    public class AsyncJob
    {
        public static StorageKey<AsyncJob> GetStorageKey(Guid id) => StorageKey<AsyncJob>.Create(id.ToString());
        public AsyncJobStatus Status { get; set; } = AsyncJobStatus.Started;
        public double Progress { get; set; } = 0;
        public Optional<string> ProgressMessage { get; set; } = new();
        public Result<object> Result { get; set; } = new();
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        public required System.Timers.Timer Timer { get; set; }
    }
}
