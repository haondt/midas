using Microsoft.Extensions.Options;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Persistence.Storages.Sqlite
{
    public class SqliteDataExportStorage : AbstractSqliteStorage, IDataExportStorage
    {
        // disable connection pooling to allow the db to be zipped after it is copied
        public SqliteDataExportStorage(IOptions<MidasPersistenceSettings> options) : base(Options.Create(options.Value with { UseConnectionPooling = false }))
        {
        }

        public void Export(string targetPath)
        {
            var targetSettings = _settings with { DatabasePath = targetPath };
            var targetStorage = new SqliteDataExportStorage(Options.Create(targetSettings));

            using var connection = GetConnection();
            using var targetConnection = targetStorage.GetConnection();

            connection.BackupDatabase(targetConnection);
        }
    }
}
