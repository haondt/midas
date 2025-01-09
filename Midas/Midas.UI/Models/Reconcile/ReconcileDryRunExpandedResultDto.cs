using Haondt.Core.Models;
using Midas.Domain.Transactions.Models;

namespace Midas.UI.Models.Reconcile
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
