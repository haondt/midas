using SpendLess.Domain.Reconcile.Models;
using SpendLess.Domain.Shared.Services;
using SpendLess.Domain.Transactions.Services;

namespace SpendLess.Domain.Reconcile.Services
{
    public class ReconcileService(
        IAsyncJobRegistry jobRegistry,
        ITransactionService transactionService
        ) : IReconcileService
    {
        public string StartDryRun(ReconcileDryRunOptions options)
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();

            _ = Task.Run(async () =>
            {
                var eligibleTransactions = await transactionService.GetTransactions(options.Filters);
            });

            return jobId;
        }
    }
}
