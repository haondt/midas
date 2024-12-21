using SpendLess.Domain.Models;

namespace SpendLess.Transactions.Models
{
    public class ExtendedTransactionDto : TransactionDto
    {
        public required string SourceAccountName { get; set; }
        public required string DestinationAccountName { get; set; }
    }
}
