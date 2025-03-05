using Haondt.Core.Models;
using Midas.Domain.Reconcile.Models;

namespace Midas.Domain.Reconcile.Services
{
    public interface IReconcileService
    {
        string StartDryRun(ReconcileDryRunOptions options);
        DetailedResult<ReconcileDryRunResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId);
        string StartMerge(string id);
        DetailedResult<DetailedResult<ReconcileMergeResultDto, string>, (double Progress, Optional<string> ProgressMessage)> GetMergeResult(string jobId);
    }
}
