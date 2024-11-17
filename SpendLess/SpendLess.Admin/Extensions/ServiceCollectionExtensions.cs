using Haondt.Web.Assets;
using Haondt.Web.Core.Components;
using SpendLess.Admin.EventHandlers;
using SpendLess.Admin.Services;
using SpendLess.Admin.SpendLess.Admin;
using SpendLess.Web.Core.Extensions;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Admin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new SpendLessNavigationItem
            {
                Title = "Administration",
                Slug = "admin",
                TypeIdentity = ComponentDescriptor<AdminModel>.TypeIdentity
            });

            services.AddScoped<ISingleEventHandler, ImportMappingsEventHandler>();

            services.AddComponent<AdminComponentDescriptorFactory>();

            services.AddSingleton<IAssetSource, ExportMappingsAssetSource>();

            return services;
        }
    }
}
