using Haondt.Web.Assets;
using SpendLess.Admin.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Admin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationLink
            {
                Title = "Administration",
                Slug = "admin",
            });

            services.AddSingleton<IAssetSource, ExportMappingsAssetSource>();
            services.AddSingleton<IAssetSource, TakeoutAssetSource>();
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IFileService, FileService>();
            services.Configure<FileSettings>(configuration.GetSection(nameof(FileSettings)));

            return services;
        }
    }
}
