using SpendLess.Domain.Models;

namespace SpendLess.TransactionImport.Models
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
