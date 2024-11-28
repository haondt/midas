using Haondt.Web.Assets;
using SpendLess.Admin.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Admin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationItem
            {
                Title = "Administration",
                Slug = "admin",
            });

            services.AddSingleton<IAssetSource, ExportMappingsAssetSource>();

            return services;
        }
    }
}
