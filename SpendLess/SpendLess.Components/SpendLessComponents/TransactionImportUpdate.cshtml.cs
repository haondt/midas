using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using SpendLess.Components.Abstractions;
using SpendLess.Domain.Models;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportUpdateModel : IComponentModel
    {
        public Optional<double> DryRunProgress { get; set; } = new();
        public Optional<string> DryRunJobId { get; set; } = new();
        public Optional<SendToNodeRedResultDto> DryRunResult { get; set; } = new();
    }
    public class TransactionImportUpdateComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<TransactionImportUpdateModel>(() => new())
            {
                ViewPath = $"~/SpendLessComponents/TransactionImportUpdate.cshtml"
            };
        }
    }

}
