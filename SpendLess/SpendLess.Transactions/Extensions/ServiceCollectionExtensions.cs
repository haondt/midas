﻿using SpendLess.Transactions.Services;
using SpendLess.Transactions.Storages;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Transactions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationItem
            {
                Title = "Transactions",
                Slug = "transactions",
            });

            services.AddSingleton<ITransactionStorage, SqliteTransactionStorage>();
            services.AddSingleton<ITransactionService, TransactionService>();
            return services;
        }
    }
}
