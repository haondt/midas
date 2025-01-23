using Midas.Core.Models;

namespace Midas.Domain.Merge.Models
{
    public class TransactionMergeDefaultValuesDto
    {
        public required (string Name, string Id) SourceAccount { get; set; }
        public required (string Name, string Id) DestinationAccount { get; set; }
        public required decimal Amount { get; set; }
        public required decimal SumAmount { get; set; }
        public required decimal MeanAmount { get; set; }
        public required string Description { get; set; }
        public required string ConcatenatedDescription { get; set; }
        public required AbsoluteDateTime Timestamp { get; set; }
        public required AbsoluteDateTime MeanTimestamp { get; set; }
        public required AbsoluteDateTime FirstTimestamp { get; set; }
        public required AbsoluteDateTime LastTimestamp { get; set; }
        public required AbsoluteDateTime CurrentTimestamp { get; set; }
        public required string Category { get; set; }
        public required List<string> Tags { get; set; }
    }
}