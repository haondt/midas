using SpendLess.Persistence.Models;

namespace SpendLess.Domain.Reconcile.Models
{
    public class ReconcileDryRunResult
    {

    }

    public class ReconcileDryRunSingleResult
    {
        public required TransactionDto NewTransaction { get; set; }
        public required List<long> OldTransactions { get; set; }
    }
}
