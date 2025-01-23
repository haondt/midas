using Haondt.Core.Models;
using Midas.Core.Constants;
using Midas.Core.Models;

namespace Midas.Domain.Merge.Models
{
    public class TransactionMergeDryRunResultDto
    {
        public Dictionary<string, string> NewAccounts { get; set; } = [];
        public Optional<string> NewCategory { get; set; } = new();
        public List<string> NewTags { get; set; } = [];
        public List<(string AccountName, bool IsMine, decimal Amount)> BalanceChanges { get; set; } = [];
        public List<long> TargetTransactions { get; set; } = [];

        public decimal Amount { get; set; }
        public List<string> Tags { get; set; } = [];
        public string Category { get; set; } = MidasConstants.DefaultCategory;
        public required AbsoluteDateTime Timestamp { get; set; }
        public required (string Id, string Name) SourceAccount { get; set; }
        public required (string Id, string Name) DestinationAccount { get; set; }
        public string Description { get; set; } = "";

    }
}
