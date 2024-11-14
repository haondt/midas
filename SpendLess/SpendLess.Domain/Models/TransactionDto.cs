using SpendLess.Domain.Constants;

namespace SpendLess.Domain.Models
{
    public class TransactionDto
    {
        public required string SourceAccountId { get; set; }
        public required string DestinationAccountId { get; set; }
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
    }
}
