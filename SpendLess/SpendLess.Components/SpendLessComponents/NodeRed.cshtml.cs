using Haondt.Web.Core.Components;
using SpendLess.Components.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class NodeRedModel : IComponentModel
    {
    }

    public class NodeRedComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<NodeRedModel>(() => new())
            {
                ViewPath = $"~/SpendLessComponents/NodeRed.cshtml",
            };
        }
    }

}
