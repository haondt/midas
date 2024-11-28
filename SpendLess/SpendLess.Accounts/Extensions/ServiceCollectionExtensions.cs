using SpendLess.Web.Domain.Models;

namespace SpendLess.Accounts.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccounts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationItem
            {
                Title = "Accounts",
                Slug = "accounts",
            });

            return services;
        }
    }
}
