using SpendLess.Domain.Reconcile.Models;

namespace SpendLess.Domain.Reconcile.Services
{
    public interface IReconcileService
    {
        string StartDryRun(ReconcileDryRunOptions options);
    }
}
