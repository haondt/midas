using Microsoft.Extensions.DependencyInjection;
using SpendLess.Domain.Services;

namespace SpendLess.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<IAsyncJobRegistry, AsyncJobRegistry>();
            return services;
        }
    }
}
