using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using SpendLess.Web.Core.Abstractions;
namespace SpendLess.Components.SpendLessComponents
{
    public class NodeRedUpdateModel : IPartialComponentModel
    {
        public Optional<string> ResponseText { get; set; } = new();
        public Optional<string> RequestText { get; set; } = new();

        public string ViewPath => NodeRedUpdateComponentDescriptorFactory.ViewPath;

        public bool IsSwap = true;
    }

    public class NodeRedUpdateComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public static readonly string ViewPath = "~/SpendLessComponents/NodeRedUpdate.cshtml";
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<NodeRedUpdateModel>
            {
                ViewPath = ViewPath
            };
        }
    }

}
