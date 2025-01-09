using Haondt.Persistence.Sqlite.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Midas.Core.Converters;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;

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

