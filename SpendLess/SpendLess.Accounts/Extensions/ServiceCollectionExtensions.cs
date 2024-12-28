using SpendLess.Accounts.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Accounts.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccounts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationSection
            {
                Label = "Accounts",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "My Accounts",
                        Slug = "accounts/mine",
                    },
                    new NavigationLink
                    {
                        Title = "All Accounts",
                        Slug = "accounts/all",
                    }
                }
            });

            services.AddSingleton<IAccountsService, AccountsService>();

            return services;
        }
    }
}
