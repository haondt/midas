using Haondt.Core.Models;
using SpendLess.Domain.Models;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportUpdateModel
    {
        public Optional<double> DryRunProgress { get; set; } = new();
        public Optional<string> DryRunJobId { get; set; } = new();
        public Optional<SendToNodeRedResultDto> DryRunResult { get; set; } = new();
    }

}
