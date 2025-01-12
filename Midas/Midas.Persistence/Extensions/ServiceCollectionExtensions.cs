using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;
using Midas.Persistence.Storages.Sqlite;

namespace Midas.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMidasPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MidasPersistenceSettings>(configuration.GetSection(nameof(MidasPersistenceSettings)));
            services.AddSingleton<SqliteMidasStorage>();
            services.AddSingleton<IDataExportStorage, SqliteDataExportStorage>();
            services.AddSingleton<IMidasStorage>(sp => sp.GetRequiredService<SqliteMidasStorage>());
            services.AddSingleton<IStorage>(sp
                => new TransientTransactionalBatchStorage(sp.GetRequiredService<SqliteMidasStorage>()));
            services.AddSingleton(typeof(ISingleTypeMidasStorage<>), typeof(SingleTypeMidasStorage<>));
            services.AddSingleton<IKvsStorage, SqliteKvsStorage>();
            services.AddSingleton<IAccountStorage, SqliteAccountStorage>();

            services.AddSingleton<ITransactionStorage, SqliteTransactionStorage>();
            services.AddSingleton<ITransactionImportDataStorage, SqliteTransactionImportDataStorage>();

            services.AddSingleton<ITransactionImportConfigurationStorage, SqliteTransactionImportConfigurationStorage>();
            return services;
        }
    }
}
