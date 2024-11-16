using Haondt.Web.Core.Components;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Domain.SpendLess.Domain
{
    public class NavigationBarModel : IComponentModel
    {
        public required List<SpendLessNavigationItem> Items { get; set; }

        public required string ActiveItemTypeIdentity { get; set; }
    }

    public class SpendLessNavigationItem
    {
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string TypeIdentity { get; set; }
    }

    public class NavigationBarComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<NavigationBarModel>()
            {
                ViewPath = $"~/SpendLess.Domain/NavigationBar.cshtml"
            };
        }
    }
}
