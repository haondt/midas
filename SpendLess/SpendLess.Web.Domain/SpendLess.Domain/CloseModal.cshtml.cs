using Haondt.Web.Core.Components;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Domain.SpendLess.Domain
{
    public class CloseModalModel : IPartialComponentModel
    {
        public string ViewPath => CloseModalModelComponentDescriptorFactory.ViewPath;
    }

    public class CloseModalModelComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public static string ViewPath = "~/SpendLess.Domain/CloseModal.cshtml";
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<CloseModalModel>(new CloseModalModel())
            {
                ViewPath = ViewPath
            };
        }
    }
}
