using Haondt.Persistence.Sqlite.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpendLess.Core.Converters;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages.Abstractions;

namespace SpendLess.Persistence.Storages.Sqlite
{
    public class SqliteSpendLessStorage : SqliteStorage, ISpendLessStorage
    {
        public SqliteSpendLessStorage(IOptions<SpendLessPersistenceSettings> options)
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
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return settings;
        }

    }
}

