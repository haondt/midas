using Haondt.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages;

namespace SpendLess.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SpendLessPersistenceSettings>(configuration.GetSection(nameof(SpendLessPersistenceSettings)));
            services.AddSingleton<SpendLessStorage>();
            services.AddSingleton<ISpendLessStorage>(sp => sp.GetRequiredService<SpendLessStorage>());
            services.AddSingleton<IStorage>(sp
                => new TransientTransactionalBatchStorage(sp.GetRequiredService<SpendLessStorage>()));
            services.AddSingleton(typeof(ISingleTypeSpendLessStorage<>), typeof(SingleTypeSpendLessStorage<>));
            services.AddSingleton<IKvsStorage, SqliteKvsStorage>();
            return services;
        }
    }
}
