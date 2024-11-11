using Haondt.Web.Core.Components;
using SpendLess.Components.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class SpendLessNavigationBarModel : IComponentModel
    {
        public List<SpendLessNavigationItem> Items { get; set; } = new List<SpendLessNavigationItem>
        {
            new()
            {
                Title = "Accounts",
                Slug = "accounts",
                TypeIdentity = ComponentDescriptor<AccountsModel>.TypeIdentity
            },
            new()
            {
                Title = "Node-Red",
                Slug = "node-red",
                TypeIdentity = ComponentDescriptor<NodeRedModel>.TypeIdentity
            },
            new()
            {
                Title = "Import Transactions",
                Slug = "transaction-import",
                TypeIdentity = ComponentDescriptor<TransactionImportModel>.TypeIdentity
            }
        };

        public required string ActiveItemTypeIdentity { get; set; }
    }

    public class SpendLessNavigationItem
    {
        public required string Title { get; set; }
        public required string Slug { get; set; }
        public required string TypeIdentity { get; set; }
    }
    public class SpendLessNavigationBarComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<SpendLessNavigationBarModel>()
            {
                ViewPath = $"~/SpendLessComponents/SpendLessNavigationBar.cshtml"
            };
        }
    }
}
