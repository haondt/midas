using Haondt.Web.Core.Components;
using SpendLess.Components.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class CloseModalModel : IComponentModel
    {
    }

    public class CloseModalModelComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<CloseModalModel>(new CloseModalModel())
            {
                ViewPath = "~/SpendLessComponents/CloseModal.cshtml"
            };
        }
    }
}
