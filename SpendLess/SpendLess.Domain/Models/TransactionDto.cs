using Newtonsoft.Json;
using SpendLess.Domain.Constants;

namespace SpendLess.Domain.Models
{
    public class TransactionDto
    {
        [JsonRequired]
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
        public string SourceAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public string DestinationAccount { get; set; } = SpendLessConstants.DefaultAccount;
        [JsonRequired]
        public required string ImportAccount { get; set; }
    }
}
