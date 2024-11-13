using Haondt.Core.Models;
using Haondt.Web.Core.Components;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportUpdateModel : IComponentModel
    {
        public Optional<double> DryRunProgress { get; set; } = new();
        public Optional<Guid> DryRunJobId { get; set; } = new();
    }
}
