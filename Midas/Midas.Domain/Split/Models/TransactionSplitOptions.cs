namespace Midas.Domain.Split.Models
{
    public class TransactionSplitOptions
    {
        public required List<TransactionSplit> Splits { get; set; }
        public required long SourceTransactionId { get; set; }
    }
}
