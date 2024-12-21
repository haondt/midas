using SpendLess.Accounts.Services;
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

            services.AddSingleton<IAccountsService, AccountsService>();

            return services;
        }
    }
}
