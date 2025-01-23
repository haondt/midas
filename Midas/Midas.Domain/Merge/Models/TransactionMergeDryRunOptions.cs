using Haondt.Core.Models;
using Midas.Core.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Merge.Models
{
    public class TransactionMergeDryRunOptions
    {
        public required List<TransactionFilter> TargetTransactionFilters { get; set; }
        public Optional<string> PreferredSourceAccountName { get; set; }
        public required string SourceAccountId { get; set; }
        public Optional<string> PreferredDestinationAccountName { get; set; }
        public required string DestinationAccountId { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required AbsoluteDateTime Timestamp { get; set; }
        public required string Category { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
