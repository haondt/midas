using Haondt.Web.Core.Components;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class AppendComponentOobSwapModel : IComponentModel
    {
        public List<OobSwap> Items { get; set; } = [];
    }

    public class OobSwap
    {
        public required IComponent Component { get; set; }
        public required string TargetSelector { get; set; }
    }

    public class AppendComponentOobSwapComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<AppendComponentOobSwapModel>()
            {
                ViewPath = $"~/SpendLessComponents/AppendComponentOobSwap.cshtml"
            };
        }
    }
}
