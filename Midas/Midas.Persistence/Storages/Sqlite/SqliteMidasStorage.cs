using Haondt.Json.Converters;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Extensions.Options;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;
using Newtonsoft.Json;

namespace Midas.Persistence.Storages.Sqlite
{
    public class SqliteMidasStorage : SqliteStorage, IMidasStorage
    {
        public SqliteMidasStorage(IOptions<MidasPersistenceSettings> options)
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

