using Haondt.Core.Models;
using SpendLess.Domain.Transactions.Models;

namespace SpendLess.UI.Models.Reconcile
{
    public class ReconcileDryRunExpandedResultDto
    {
        public required Result<List<ReconcileDryRunExpandedSingleResult>, string> MergedTransactions { get; set; }
    }

    public class ReconcileDryRunExpandedSingleResult
    {
        public required ExtendedTransactionDto NewTransaction { get; set; }
        public required List<long> OldTransactions { get; set; }
    }
}
