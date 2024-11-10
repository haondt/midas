using Haondt.Web.Core.Components;
using Haondt.Web.Services;
using SpendLess.Components.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class SpendLessLayoutModel : IComponentModel
    {
        public required IComponent Content { get; set; }
        public required IComponent NavigationBar { get; set; }
    }

    public class SpendLessLayoutComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<SpendLessLayoutModel>()
            {
                ViewPath = $"~/SpendLessComponents/SpendLessLayout.cshtml",
            };
        }
    }

    public class SpendLessLayoutComponentFactory(IComponentFactory componentFactory) : ILayoutComponentFactory
    {

        public async Task<IComponent> GetLayoutAsync(IComponent content, string componentIdentity)
        {
            var navigationBar = await componentFactory.GetPlainComponent(new SpendLessNavigationBarModel
            {
                ActiveItemTypeIdentity = componentIdentity
            });
            return await componentFactory.GetPlainComponent(new SpendLessLayoutModel
            {
                Content = content,
                NavigationBar = navigationBar
            });
        }
    }
}
