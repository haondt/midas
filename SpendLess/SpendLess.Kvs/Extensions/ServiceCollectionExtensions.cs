using SpendLess.Kvs.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Kvs.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKvs(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationItem
            {
                Title = "Key-Value Store",
                Slug = "kvs",
            });

            services.AddSingleton<IKvsService, KvsService>();

            return services;
        }

    }
}
