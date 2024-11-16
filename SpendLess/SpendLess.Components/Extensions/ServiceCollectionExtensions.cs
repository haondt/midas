using Haondt.Web.Core.Components;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Web.Core.Extensions;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Components.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddSpendLessComponents(this IServiceCollection services)
        {
            services.AddCoreComponents();

            services.AddSingleton(new SpendLessNavigationItem
            {
                Title = "Accounts",
                Slug = "accounts",
                TypeIdentity = ComponentDescriptor<AccountsModel>.TypeIdentity
            });
            services.AddSingleton(new SpendLessNavigationItem
            {
                Title = "Import Transactions",
                Slug = "transaction-import",
                TypeIdentity = ComponentDescriptor<TransactionImportModel>.TypeIdentity
            });
            services.AddSingleton(new SpendLessNavigationItem
            {
                Title = "Node-Red",
                Slug = "node-red",
                TypeIdentity = ComponentDescriptor<NodeRedModel>.TypeIdentity
            });

            return services;
        }

        private static IServiceCollection AddCoreComponents(this IServiceCollection services)
        {
            services.AddComponent<AppendComponentOobSwapComponentDescriptorFactory>();
            services.AddComponent<ErrorComponentDescriptorFactory>();

            services.AddComponent<AccountsComponentDescriptorFactory>();
            services.AddComponent<UpsertAccountComponentDescriptorFactory>();

            services.AddComponent<NodeRedComponentDescriptorFactory>();
            services.AddComponent<NodeRedUpdateComponentDescriptorFactory>();

            services.AddComponent<TransactionImportComponentDescriptorFactory>();
            services.AddComponent<UpsertTransactionImportConfigurationModalComponentDescriptorFactory>();
            services.AddComponent<TransactionImportUpdateComponentDescriptorFactory>();
            services.AddComponent<TransactionImportSendToNodeRedResultComponentDescriptorFactory>();

            return services;
        }
    }
}
