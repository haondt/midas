using SpendLess.Domain.Services;
using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Services
{
    public class ReconcileService(
        IAsyncJobRegistry jobRegistry
        ) : IReconcileService
    {
        public string StartDryRun(ReconcileDryRunOptions options)
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();

            _ = Task.Run(async () =>
            {

            });

            return jobId;
        }
    }
}
