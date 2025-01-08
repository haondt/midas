using SpendLess.Domain.Transactions.Models;

namespace SpendLess.UI.Models.Reconcile
{
    public class ReconcileDryRunExpandedResultDto
    {
        public List<ReconcileDryRunExpandedSingleResult> MergedTransactions { get; set; } = [];
    }

    public class ReconcileDryRunExpandedSingleResult
    {
        public required ExtendedTransactionDto NewTransaction { get; set; }
        public required List<long> OldTransactions { get; set; }
    }
}
