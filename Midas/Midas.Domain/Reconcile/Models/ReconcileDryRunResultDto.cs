using Haondt.Core.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Reconcile.Models
{
    public class ReconcileDryRunResultDto
    {
        public required DetailedResult<List<ReconcileDryRunSingleResult>, string> MergedTransactions { get; set; }
    }

    public class ReconcileDryRunSingleResult
    {
        public required TransactionDto NewTransaction { get; set; }
        public required List<long> OldTransactions { get; set; }
    }
}
