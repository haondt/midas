using Haondt.Core.Extensions;
using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendLess.Persistence.Services;

namespace SpendLess.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SpendLessPersistenceSettings>(configuration.GetSection(nameof(SpendLessPersistenceSettings)));
            var persistenceSettings = configuration.GetSection<SpendLessPersistenceSettings>();
            switch (persistenceSettings.Driver)
            {
                case SpendLessPersistenceDrivers.File:
                    services.AddSingleton<SpendLessFileStorage>();
                    services.AddSingleton<IStorage>(sp
                        => new TransientTransactionalBatchStorage(sp.GetRequiredService<SpendLessFileStorage>()));
                    break;
                case SpendLessPersistenceDrivers.Postgres:
                    //services.AddSingleton<ISpendLessStorage, SpendLessPostgresqlStorage>();
                    throw new NotImplementedException();
                    break;
            }
            services.AddSingleton(typeof(ISingleTypeSpendLessStorage<>), typeof(SingleTypeSpendLessStorage<>));
            return services;
        }
    }
}
