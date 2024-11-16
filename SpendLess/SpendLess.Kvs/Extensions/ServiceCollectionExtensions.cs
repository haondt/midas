using Haondt.Web.Core.Components;
using SpendLess.Kvs.Services;
using SpendLess.Kvs.SpendLess.Kvs;
using SpendLess.Web.Core.Extensions;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Kvs.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKvs(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new SpendLessNavigationItem
            {
                Title = "Key-Value Store",
                Slug = "kvs",
                TypeIdentity = ComponentDescriptor<KvsModel>.TypeIdentity
            });

            services.AddComponent<KvsComponentDescriptorFactory>();

            services.AddScoped<ISingleEventHandler, AutocompleteEventHandler>();
            services.AddScoped<ISingleEventHandler, UpsertEventHandler>();
            services.AddSingleton<IKvsService, KvsService>();

            return services;
        }

    }
}
