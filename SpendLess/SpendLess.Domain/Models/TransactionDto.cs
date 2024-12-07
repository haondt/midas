using SpendLess.Domain.Constants;

namespace SpendLess.Domain.Models
{
    public class TransactionDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
        public string SourceAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public string Description { get; set; } = "";
        public string DestinationAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public required string ImportAccount { get; set; }
        public required List<string> SourceData { get; set; }
    }
}
