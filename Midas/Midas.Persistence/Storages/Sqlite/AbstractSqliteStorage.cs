using Haondt.Json.Converters;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Midas.Persistence.Services;
using Newtonsoft.Json;

namespace Midas.Persistence.Storages.Sqlite
{
    public abstract class AbstractSqliteStorage
    {
        protected MidasPersistenceSettings _settings;
        protected readonly JsonSerializerSettings _serializerSettings;
        protected readonly string _connectionString;
        protected AbstractSqliteStorage(IOptions<MidasPersistenceSettings> options)
        {
            _settings = options.Value;
            _serializerSettings = new JsonSerializerSettings();
            _connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = _settings.DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                Pooling = _settings.UseConnectionPooling
            }.ToString();
            ConfigureSerializerSettings(_serializerSettings);
        }

        protected virtual JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Formatting = Formatting.None;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new GenericStorageKeyJsonConverter());
            settings.Converters.Add(new GenericOptionalJsonConverter());
            return settings;
        }

        protected string SanitizeTableName(string tableName)
        {
            // Escape any double quotes by replacing them with two double quotes
            var sanitized = tableName.Replace("\"", "\"\"");

            // Surround the sanitized table name with double quotes
            return $"\"{sanitized}\"";
        }

        protected virtual SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            try
            {
                using var walCommand = connection.CreateCommand();
                walCommand.CommandText = "PRAGMA journal_mode=WAL;";
                walCommand.ExecuteNonQuery();

                using var enableForeignKeysCommand = new SqliteCommand("PRAGMA foreign_keys = ON;", connection);
                enableForeignKeysCommand.ExecuteNonQuery();
            }
            catch
            {
                connection.Close();
                throw;
            }

            return connection;
        }
        protected virtual (SqliteConnection, SqliteTransaction) GetTransactionalConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                using var walCommand = new SqliteCommand("PRAGMA journal_mode=WAL;", connection, transaction);
                walCommand.ExecuteNonQuery();

                using var enableForeignKeysCommand = new SqliteCommand("PRAGMA foreign_keys = ON;", connection, transaction);
                enableForeignKeysCommand.ExecuteNonQuery();

                return (connection, transaction);
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                finally
                {
                    connection.Dispose();
                }
                throw;
            }
        }

        protected void WithConnection(Action<SqliteConnection> action)
        {
            using var connection = GetConnection();
            action(connection);
        }

        protected T WithConnection<T>(Func<SqliteConnection, T> action)
        {
            using var connection = GetConnection();
            return action(connection);
        }

        protected void WithTransaction(Action<SqliteConnection, SqliteTransaction> action)
        {
            var (connection, transaction) = GetTransactionalConnection();
            try
            {
                action(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                finally
                {
                    try
                    {
                        transaction.Dispose();
                    }
                    finally
                    {
                        connection.Dispose();
                    }
                }
                throw;
            }
        }

        protected T WithTransaction<T>(Func<SqliteConnection, SqliteTransaction, T> action)
        {
            var (connection, transaction) = GetTransactionalConnection();
            try
            {
                var result = action(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                finally
                {
                    try
                    {
                        transaction.Dispose();
                    }
                    finally
                    {
                        connection.Dispose();
                    }
                }
                throw;
            }
        }

    }
}
