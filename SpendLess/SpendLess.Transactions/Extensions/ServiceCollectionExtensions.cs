using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendLess.Transactions.Services;
using SpendLess.Transactions.Storages;

namespace SpendLess.Transactions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITransactionStorage, SqliteTransactionStorage>();
            services.AddSingleton<ITransactionService, TransactionService>();
            return services;
        }
    }
}
