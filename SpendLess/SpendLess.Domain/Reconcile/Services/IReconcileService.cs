using Haondt.Core.Models;
using SpendLess.Domain.Reconcile.Models;

namespace SpendLess.Domain.Reconcile.Services
{
    public interface IReconcileService
    {
        string StartDryRun(ReconcileDryRunOptions options);
        Result<ReconcileDryRunResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId);
    }
}
