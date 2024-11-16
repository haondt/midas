using Haondt.Web.Core.Components;
using Haondt.Web.Services;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Domain.SpendLess.Domain
{
    public class LayoutModel : IComponentModel
    {
        public required IComponent Content { get; set; }
        public required IComponent NavigationBar { get; set; }
    }

    public class LayoutComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<LayoutModel>()
            {
                ViewPath = $"~/SpendLess.Domain/Layout.cshtml",
            };
        }
    }

    public class LayoutComponentFactory(IComponentFactory componentFactory, IEnumerable<SpendLessNavigationItem> navigationItems) : ILayoutComponentFactory
    {

        public async Task<IComponent> GetLayoutAsync(IComponent content, string componentIdentity)
        {
            var navigationBar = await componentFactory.GetPlainComponent(new NavigationBarModel
            {
                ActiveItemTypeIdentity = componentIdentity,
                Items = navigationItems.ToList()
            });
            return await componentFactory.GetPlainComponent(new LayoutModel
            {
                Content = content,
                NavigationBar = navigationBar
            });
        }
    }
}
