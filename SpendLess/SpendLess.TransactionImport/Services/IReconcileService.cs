using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Services
{
    public interface IReconcileService
    {
        string StartDryRun(ReconcileDryRunOptions options);
    }
}
