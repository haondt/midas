using Haondt.Persistence.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Persistence.Storages.Sqlite
{
    class SqliteTransactionImportConfigurationStorage : AbstractSqliteStorage, ITransactionImportConfigurationStorage
    {
        private readonly string _tableName;
        public SqliteTransactionImportConfigurationStorage(IOptions<MidasPersistenceSettings> options) : base(options)
        {
            _tableName = SanitizeTableName(options.Value.TransactionsImportConfigurationTableName);
            InitializeDb();
        }
        protected virtual void InitializeDb()
        {
            if (WithConnection(connection =>
            {
                var checkTableQuery = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = {_tableName};";
                using var checkTableCommand = new SqliteCommand(checkTableQuery, connection);
                return checkTableCommand.ExecuteScalar() != null;
            }))
                return;

            WithTransaction((connection, transaction) =>
            {
                using var createPrimaryTableCommand = new SqliteCommand(
                    $@"
                        CREATE TABLE {_tableName} (
                        slug TEXT PRIMARY KEY NOT NULL
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }

        public Task Add(string slug) => AddMany([slug]);

        public Task AddMany(List<string> slugs)
        {
            var query = $"INSERT INTO {_tableName} (slug) VALUES (@slug) ON CONFLICT (slug) DO NOTHING;";
            WithTransaction((conn, trns) =>
            {
                var command = new SqliteCommand(query, conn, trns);
                var slugParameter = command.Parameters.Add("@slug", SqliteType.Text);

                foreach (var slug in slugs)
                {
                    slugParameter.Value = slug;
                    command.ExecuteNonQuery();
                }
            });

            return Task.CompletedTask;
        }


        public Task Synchronize(List<string> slugs)
        {
            WithTransaction((connection, transaction) =>
            {
                var deleteQuery = $@"
                    DELETE FROM {_tableName}
                    WHERE slug NOT IN ({string.Join(", ", slugs.Select((_, i) => $"@slug{i}"))});";

                using var deleteCommand = new SqliteCommand(deleteQuery, connection, transaction);
                for (int i = 0; i < slugs.Count; i++)
                    deleteCommand.Parameters.AddWithValue($"@slug{i}", slugs[i]);
                deleteCommand.ExecuteNonQuery();

                var insertQuery = $@"
                    INSERT INTO {_tableName} (slug)
                    VALUES (@slug)
                    ON CONFLICT (slug) DO NOTHING;";

                using var insertCommand = new SqliteCommand(insertQuery, connection, transaction);
                var slugParameter = insertCommand.Parameters.Add("@slug", SqliteType.Text);

                foreach (var slug in slugs)
                {
                    slugParameter.Value = slug;
                    insertCommand.ExecuteNonQuery();
                }
            });

            return Task.CompletedTask;
        }

        public Task<List<string>> GetAll()
        {
            var result = WithConnection(connection =>
            {
                var query = $"SELECT slug FROM {_tableName};";
                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                var slugs = new List<string>();
                while (reader.Read())
                    slugs.Add(reader["slug"] as string ?? throw new StorageException("Unable to retrieve slug from the database."));

                return slugs;
            });

            return Task.FromResult(result);
        }

    }
}
