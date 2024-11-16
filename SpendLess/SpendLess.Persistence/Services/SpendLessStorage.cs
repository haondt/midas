using Haondt.Persistence.Sqlite.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpendLess.Persistence.Converters;

namespace SpendLess.Persistence.Services
{
    public class SpendLessStorage : SqliteStorage, ISpendLessStorage
    {
        public SpendLessStorage(IOptions<SpendLessPersistenceSettings> options)
            : base(Options.Create(new SqliteStorageSettings
            {
                DatabasePath = options.Value.DatabasePath,
                ForeignKeyTableName = options.Value.ForeignKeyTableName,
                PrimaryTableName = options.Value.PrimaryTableName,
                StoreKeyStrings = true
            }))
        {
        }

        protected override JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings = base.ConfigureSerializerSettings(settings);
            settings.Converters.Add(new GenericOptionalJsonConverter());
            return settings;
        }

    }
}

