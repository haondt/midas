using Haondt.Core.Models;
using SpendLess.Domain.Constants;
using SpendLess.TransactionImport.Models;

namespace SpendLess.Domain.Models
{
    public class SendToNodeRedResultDto
    {
        public List<SendToNodeRedSingleResultDto> Transactions { get; set; } = [];
        public Dictionary<string, string> NewAccounts { get; set; } = [];
        public Dictionary<string, int> NewCategories { get; set; } = [];
        public Dictionary<string, int> NewTags { get; set; } = [];
        public decimal BalanceChange { get; set; } = 0;
        public required SendToNodeRedResultAccountDataDto ImportAccount { get; set; }
    }

    public class SendToNodeRedSingleResultDto
    {
        public required string SourceRequestPayload { get; set; }
        public HashSet<string> Warnings { get; set; } = [];
        public HashSet<string> Errors { get; set; } = [];
        public string Status => Errors.Count > 0 ? TransactionImportStatus.Error
            : Warnings.Count > 0 ? TransactionImportStatus.Warning
            : TransactionImportStatus.Success;
        public Optional<SendToNodeRedTransactionResultDto> Transaction { get; set; } = new();
    }

    public class SendToNodeRedTransactionResultDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
        public required SendToNodeRedResultAccountDataDto Source { get; set; }
        public required SendToNodeRedResultAccountDataDto Destination { get; set; }
        public string Description { get; set; } = "";
    }

    public class SendToNodeRedResultAccountDataDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}
