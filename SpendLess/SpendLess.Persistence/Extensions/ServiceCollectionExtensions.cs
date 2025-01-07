using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages.Abstractions;
using SpendLess.Persistence.Storages.Sqlite;

namespace SpendLess.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SpendLessPersistenceSettings>(configuration.GetSection(nameof(SpendLessPersistenceSettings)));
            services.AddSingleton<SqliteSpendLessStorage>();
            services.AddSingleton<IDataExportStorage, SqliteDataExportStorage>();
            services.AddSingleton<ISpendLessStorage>(sp => sp.GetRequiredService<SqliteSpendLessStorage>());
            services.AddSingleton<IStorage>(sp
                => new TransientTransactionalBatchStorage(sp.GetRequiredService<SqliteSpendLessStorage>()));
            services.AddSingleton(typeof(ISingleTypeSpendLessStorage<>), typeof(SingleTypeSpendLessStorage<>));
            services.AddSingleton<IKvsStorage, SqliteKvsStorage>();
            services.AddSingleton<IAccountStorage, SqliteAccountStorage>();

            services.AddSingleton<ITransactionStorage, SqliteTransactionStorage>();
            services.AddSingleton<ITransactionImportDataStorage, SqliteTransactionImportDataStorage>();
            return services;
        }
    }
}
