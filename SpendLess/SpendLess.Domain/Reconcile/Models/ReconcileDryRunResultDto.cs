using Haondt.Core.Models;
using SpendLess.Persistence.Models;

namespace SpendLess.Domain.Reconcile.Models
{
    public class ReconcileDryRunResultDto
    {
        public required Result<List<ReconcileDryRunSingleResult>, string> MergedTransactions { get; set; }
    }

    public class ReconcileDryRunSingleResult
    {
        public required TransactionDto NewTransaction { get; set; }
        public required List<long> OldTransactions { get; set; }
    }
}
