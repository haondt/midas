using Midas.Persistence.Models;

namespace Midas.Domain.Transactions.Models
{
    public class ExtendedTransactionDto : TransactionDto
    {
        public required string SourceAccountName { get; set; }
        public required string DestinationAccountName { get; set; }
    }
}
