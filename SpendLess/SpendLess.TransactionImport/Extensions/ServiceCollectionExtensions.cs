using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendLess.TransactionImport.Models;
using SpendLess.TransactionImport.Services;
using SpendLess.TransactionImport.Storages;
using SpendLess.Web.Domain.Models;

namespace SpendLess.TransactionImport.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddTransactionImport(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationSection
            {
                Label = "Transactions",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "Import",
                        Slug = "transaction-import",
                    },
                    new NavigationLink
                    {
                        Title = "Re-Import",
                        Slug = "transaction-import/reimport",
                    },
                    new NavigationLink
                    {
                        Title = "Reconcile",
                        Slug = "reconcile",
                    }
                }
            });

            services.AddSingleton<ITransactionImportService, TransactionImportService>();
            services.AddSingleton<IReconcileService, ReconcileService>();
            services.AddSingleton<ITransactionImportDataStorage, SqliteTransactionImportDataStorage>();
            services.Configure<TransactionImportSettings>(configuration.GetSection(nameof(TransactionImportSettings)));

            return services;
        }
    }
}
