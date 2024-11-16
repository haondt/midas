using Microsoft.Extensions.DependencyInjection;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComponent<T>(this IServiceCollection services) where T : IComponentDescriptorFactory
        {
            return services.AddScoped(sp => ActivatorUtilities.CreateInstance<T>(sp).Create());
        }

    }
}
