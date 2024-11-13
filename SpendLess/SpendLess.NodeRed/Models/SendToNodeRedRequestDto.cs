using SpendLess.Domain.Models;

namespace SpendLess.NodeRed.Models
{
    public class SendToNodeRedRequestDto
    {
        public Guid AccountId { get; set; }
        public TransactionImportConfigurationDto Options { get; set; } = new();
        public List<string> Csv { get; set; } = [];
    }
}
