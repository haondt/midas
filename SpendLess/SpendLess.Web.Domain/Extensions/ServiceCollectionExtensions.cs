using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Services;
using SpendLess.Components.Services;
using SpendLess.Web.Core.Extensions;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Web.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddSpendLessWebDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILayoutComponentFactory, LayoutComponentFactory>();
            services.AddSingleton<ISingletonComponentFactory, SingletonComponentFactory>();
            services.AddScoped<IEventHandler, SingleEventHandlerRegistry>();

            services.AddSpendLessDomainWebComponents();

            services.AddSpendLessHeadEntries();

            return services;
        }

        private static IServiceCollection AddSpendLessHeadEntries(this IServiceCollection services)
        {
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://kit.fontawesome.com/afd44816da.js",
                CrossOrigin = "anonymous"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new MetaDescriptor
            {
                Name = "htmx-config",
                Content = @"{
                    ""responseHandling"": [
                        { ""code"": ""204"", ""swap"": false },
                        { ""code"": "".*"", ""swap"": true }
                    ],
                    ""scrollIntoViewOnBoost"": false
                }",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = "https://unpkg.com/htmx-ext-multi-swap@2.0.0/multi-swap.js"
            });
            return services;
        }

        private static IServiceCollection AddSpendLessDomainWebComponents(this IServiceCollection services)
        {
            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSAssetSources();

            services.AddComponent<LayoutComponentDescriptorFactory>();
            services.AddComponent<NavigationBarComponentDescriptorFactory>();
            services.AddComponent<AutocompleteComponentDescriptorFactory>();
            services.AddComponent<CloseModalModelComponentDescriptorFactory>();
            services.AddComponent<ToastComponentDescriptorFactory>();

            return services;
        }
    }
}
