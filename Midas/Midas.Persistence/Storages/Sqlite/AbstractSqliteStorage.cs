using Haondt.Persistence.Converters;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Midas.Core.Converters;
using Midas.Persistence.Services;

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

            using var walCommand = connection.CreateCommand();
            walCommand.CommandText = "PRAGMA journal_mode=WAL;";
            walCommand.ExecuteNonQuery();

            using var enableForeignKeysCommand = new SqliteCommand("PRAGMA foreign_keys = ON;", connection);
            enableForeignKeysCommand.ExecuteNonQuery();

            return connection;
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
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                action(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        protected T WithTransaction<T>(Func<SqliteConnection, SqliteTransaction, T> action)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                var result = action(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

    }
}
