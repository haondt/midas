using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Services;
using SpendLess.Components.Abstractions;
using SpendLess.Components.Services;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Components.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComponent<T>(this IServiceCollection services) where T : IComponentDescriptorFactory
        {
            return services.AddScoped(sp => ActivatorUtilities.CreateInstance<T>(sp).Create());
        }

        public static IServiceCollection AddSpendLessComponentServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSAssetSources();

            services.AddScoped<ILayoutComponentFactory, SpendLessLayoutComponentFactory>();
            services.AddSingleton<ISingletonComponentFactory, SingletonComponentFactory>();

            return services;
        }

        public static IServiceCollection AddSpendLessComponents(this IServiceCollection services)
        {
            services.AddCoreComponents();
            return services;
        }

        private static IServiceCollection AddCoreComponents(this IServiceCollection services)
        {
            services.AddComponent<AppendComponentOobSwapComponentDescriptorFactory>();
            services.AddComponent<SpendLessLayoutComponentDescriptorFactory>();
            services.AddComponent<SpendLessNavigationBarComponentDescriptorFactory>();
            services.AddComponent<ToastComponentDescriptorFactory>();
            services.AddComponent<ErrorComponentDescriptorFactory>();
            services.AddComponent<CloseModalModelComponentDescriptorFactory>();

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
