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


            //services.AddScoped<ISingleEventHandler, AutocompleteEventHandler>();
            //services.AddScoped<ISingleEventHandler, UpsertEventHandler>();
            //services.AddScoped<ISingleEventHandler, AddAliasEventHandler>();
            //services.AddScoped<ISingleEventHandler, RemoveAliasEventHandler>();

            services.AddSingleton<IKvsService, KvsService>();

            return services;
        }

    }
}
