using SpendLess.Core.Constants;
using SpendLess.Core.Models;

namespace SpendLess.Persistence.Models
{
    public class TransactionDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
        public string SourceAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public string DestinationAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public string Description { get; set; } = "";
        public required AbsoluteDateTime TimeStamp { get; set; }
    }
}
