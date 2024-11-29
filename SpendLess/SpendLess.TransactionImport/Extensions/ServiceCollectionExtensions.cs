using Microsoft.Extensions.DependencyInjection;
using SpendLess.Web.Domain.Models;

namespace SpendLess.TransactionImport.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddTransactionImport(this IServiceCollection services)
        {
            services.AddSingleton(new NavigationItem
            {
                Title = "Import Transactions",
                Slug = "transaction-import",
            });

            return services;
        }
    }
}
