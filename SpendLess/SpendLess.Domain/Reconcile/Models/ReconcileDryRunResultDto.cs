using SpendLess.Persistence.Models;

namespace SpendLess.Domain.Reconcile.Models
{
    public class ReconcileDryRunResultDto
    {
        public List<ReconcileDryRunSingleResult> MergedTransactions { get; set; } = [];
    }

    public class ReconcileDryRunSingleResult
    {
        public required TransactionDto NewTransaction { get; set; }
        public required List<long> OldTransactions { get; set; }
    }
}
