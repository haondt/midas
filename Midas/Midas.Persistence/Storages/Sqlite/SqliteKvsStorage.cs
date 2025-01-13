using Haondt.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Midas.Persistence.Exceptions;
using Midas.Persistence.Extensions;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Persistence.Storages.Sqlite
{
    public class SqliteKvsStorage : AbstractSqliteStorage, IKvsStorage
    {
        private readonly string _kvsTableName;

        public SqliteKvsStorage(IOptions<MidasPersistenceSettings> options) : base(options)
        {
            _kvsTableName = SanitizeTableName(options.Value.KvsTableName);
            InitializeDb();
        }

        protected virtual void InitializeDb()
        {

            if (WithConnection(connection =>
            {
                var checkTableQuery = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = {_kvsTableName};";
                using var checkTableCommand = new SqliteCommand(checkTableQuery, connection);
                return checkTableCommand.ExecuteScalar() != null;
            }))
                return;

            WithTransaction((connection, transaction) =>
            {
                using var createPrimaryTableCommand = new SqliteCommand(
                    $@"
                        CREATE TABLE {_kvsTableName} (
                        id TEXT PRIMARY KEY NOT NULL,
                        isAliasFor TEXT, 
                        value TEXT,
                        FOREIGN KEY (isAliasFor) REFERENCES {_kvsTableName}(id) ON DELETE CASCADE
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }

        private string? InternalGetKeyFromAlias(string alias, SqliteConnection connection, SqliteTransaction transaction)
        {
            var aliasCheckQuery = $"SELECT isAliasFor FROM {_kvsTableName} WHERE id = @key;";
            using var aliasCheckCommand = new SqliteCommand(aliasCheckQuery, connection, transaction);
            aliasCheckCommand.Parameters.AddWithValue("@key", alias);
            var isAliasFor = aliasCheckCommand.ExecuteScalar() as string;
            return isAliasFor;
        }

        private string? InternalGetValueFromKey(string key, SqliteConnection connection, SqliteTransaction transaction)
        {
            var query = $"SELECT value FROM {_kvsTableName} WHERE id = @key;";
            using var aliasKeyCheckCommand = new SqliteCommand(query, connection, transaction);
            aliasKeyCheckCommand.Parameters.AddWithValue("@key", key);
            var value = aliasKeyCheckCommand.ExecuteScalar() as string;
            return value;
        }

        private (string? IsAliasFor, string? Value) InternalGetKeyAndValue(string keyOrAlias, SqliteConnection connection, SqliteTransaction transaction)
        {
            return InternalGetKeysAndValues([keyOrAlias], connection, transaction)[0];
        }

        private List<(string? IsAliasFor, string? Value)> InternalGetKeysAndValues(List<string> keysOrAliases, SqliteConnection connection, SqliteTransaction transaction)
        {
            var query = $"SELECT isAliasFor, value FROM {_kvsTableName} WHERE id = @keyOrAlias;";
            using var command = new SqliteCommand(query, connection, transaction);
            var keyOrAliasParameter = command.Parameters.Add("@keyOrAlias", SqliteType.Text);
            List<(string?, string?)> results = [];
            foreach (var keyOrAlias in keysOrAliases)
            {
                keyOrAliasParameter.Value = keyOrAlias;
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var isAliasFor = reader["isAliasFor"] as string;
                    var value = reader["value"] as string;
                    results.Add((isAliasFor, value));
                }
                else
                {
                    results.Add((null, null));
                }
            }
            return results;
        }


        public Task UpsertKeyAndValue(string key, string value)
        {
            WithTransaction((connection, transaction) =>
            {
                // Check if the key is already used as an alias
                var isAliasFor = InternalGetKeyFromAlias(key, connection, transaction);
                if (isAliasFor != null)
                    throw new StorageException($"The value {key} is already being used as an alias for the key {isAliasFor}.");

                // Upsert the key-value pair
                var upsertQuery = $@"
                    INSERT INTO {_kvsTableName} (id, value)
                    VALUES (@key, @value)
                    ON CONFLICT(id) DO UPDATE SET value = excluded.value;";
                using var upsertCommand = new SqliteCommand(upsertQuery, connection, transaction);
                upsertCommand.Parameters.AddWithValue("@key", key);
                upsertCommand.Parameters.AddWithValue("@value", value);
                upsertCommand.ExecuteNonQuery();
            });

            return Task.CompletedTask;
        }

        public Task<List<string>> AddAlias(string key, string alias)
        {
            var result = WithTransaction((connection, transaction) =>
            {
                var (isAliasFor, keyValue) = InternalGetKeyAndValue(alias, connection, transaction);
                if (keyValue != null)
                    throw new StorageException($"The value {alias} is already being used as a key.");
                if (isAliasFor != null)
                    throw new StorageException($"The Alias {alias} is already mapped to the key {isAliasFor}.");

                var aliasedValue = InternalGetValueFromKey(key, connection, transaction);
                if (aliasedValue == null)
                {
                    var upsertQuery = $@"
                        INSERT INTO {_kvsTableName} (id, value)
                        VALUES (@key, @value)
                        ON CONFLICT(id) DO UPDATE SET value = excluded.value;";
                    using var upsertCommand = new SqliteCommand(upsertQuery, connection, transaction);
                    upsertCommand.Parameters.AddWithValue("@key", key);
                    upsertCommand.Parameters.AddWithValue("@value", "");
                    upsertCommand.ExecuteNonQuery();
                }

                var insertAliasQuery = $@"
                    INSERT INTO {_kvsTableName} (id, isAliasFor)
                    VALUES (@alias, @key);";
                using var insertAliasCommand = new SqliteCommand(insertAliasQuery, connection, transaction);
                insertAliasCommand.Parameters.AddWithValue("@alias", alias);
                insertAliasCommand.Parameters.AddWithValue("@key", key);
                insertAliasCommand.ExecuteNonQuery();

                return InternalGetAliases(key, connection, transaction);
            });

            return Task.FromResult(result);
        }


        public Task<List<string>> DeleteAlias(string alias)
        {
            var result = WithTransaction((connection, transaction) =>
            {
                var (isAliasFor, value) = InternalGetKeyAndValue(alias, connection, transaction);
                if (value != null || isAliasFor == null)
                    return [];

                var query = $@"
                    DELETE FROM {_kvsTableName}
                    WHERE id = @alias;";
                using var command = new SqliteCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@alias", alias);
                command.ExecuteNonQuery();

                return InternalGetAliases(isAliasFor, connection, transaction);
            });

            return Task.FromResult(result);
        }

        public Task MoveMapping(string oldKey, string newKey)
        {
            WithTransaction((connection, transaction) =>
            {
                var value = InternalGetValueFromKey(oldKey, connection, transaction);
                if (value == null)
                    throw new InvalidOperationException($"No such mapping: {oldKey}");

                var existing = InternalGetKeyAndValue(newKey, connection, transaction);
                if (existing.IsAliasFor != null || existing.Value != null)
                    throw new InvalidOperationException($"Key {newKey} is already being used.");

                var insertCommand = new SqliteCommand(
                    $@"
                    INSERT INTO {_kvsTableName} (id, isAliasFor, value)
                    VALUES (@key, NULL, @value)", connection, transaction);
                insertCommand.Parameters.AddWithValue("@key", newKey);
                insertCommand.Parameters.AddWithValue("@value", value);
                insertCommand.ExecuteNonQuery();

                var updateAliasesCommand = new SqliteCommand($@"
                    UPDATE {_kvsTableName}
                    SET isAliasFor = @newKey
                    WHERE isAliasFor = @oldKey;
                ", connection, transaction);
                updateAliasesCommand.Parameters.AddWithValue("@newKey", newKey);
                updateAliasesCommand.Parameters.AddWithValue("@oldKey", oldKey);
                updateAliasesCommand.ExecuteNonQuery();

                var deleteCommand = new SqliteCommand($@"
                    DELETE FROM {_kvsTableName}
                    WHERE id = @key;
                ", connection, transaction);
                deleteCommand.Parameters.AddWithValue("@key", oldKey);
                deleteCommand.ExecuteNonQuery();
            });

            return Task.CompletedTask;
        }

        public Task DeleteKey(string key)
        {
            WithTransaction((connection, transaction) =>
            {
                var isAliasFor = InternalGetKeyFromAlias(key, connection, transaction);
                if (isAliasFor != null)
                    return;

                var query = $@"
                    DELETE FROM {_kvsTableName}
                    WHERE id = @key;";
                using var command = new SqliteCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@key", key);
                command.ExecuteNonQuery();
            });

            return Task.CompletedTask;
        }

        public Task<Optional<string>> TryGetValueFromKeyOrAlias(string keyOrAlias)
        {
            var result = WithConnection(connection =>
            {
                var query = $@"
                    SELECT value FROM {_kvsTableName}
                    WHERE id = @keyOrAlias OR id = (SELECT isAliasFor FROM {_kvsTableName} WHERE id = @keyOrAlias);";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@keyOrAlias", keyOrAlias);
                var result = command.ExecuteScalar() as string;
                return result;
            });

            return Task.FromResult(result == null ? new Optional<string>() : (string)result);
        }
        public Task<Optional<(string Key, string Value)>> TryGetKeyAndValueFromKeyOrAlias(string keyOrAlias)
        {
            var result = WithTransaction((connection, transaction) =>
            {
                var (isAliasFor, keyValue) = InternalGetKeyAndValue(keyOrAlias, connection, transaction);
                if (keyValue != null)
                    return new((keyOrAlias, keyValue));
                if (isAliasFor == null)
                    return new Optional<(string, string)>();
                var aliasedValue = InternalGetValueFromKey(isAliasFor, connection, transaction);
                return new((isAliasFor, aliasedValue ?? string.Empty));
            });

            return Task.FromResult(result);
        }

        public Task<Optional<string>> TryGetKeyFromKeyOrAlias(string keyOrAlias)
        {
            var result = WithTransaction((connection, transaction) =>
            {
                var (isAliasFor, keyValue) = InternalGetKeyAndValue(keyOrAlias, connection, transaction);
                if (keyValue != null)
                    return keyOrAlias;
                if (isAliasFor != null)
                    return isAliasFor;
                return null;
            });

            return Task.FromResult(result == null ? new Optional<string>() : (string)result);
        }

        public Task<List<string>> SearchKey(string partialKeyOrAlias)
        {
            var result = WithConnection(connection =>
            {
                var query = $@"
                    SELECT DISTINCT COALESCE(isAliasFor, id) FROM {_kvsTableName}
                    WHERE id LIKE @partialKeyOrAlias
                    LIMIT {_settings.MaxKvsSearchHits};";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddLikeTermWithValue("@partialKeyOrAlias", partialKeyOrAlias);

                using var reader = command.ExecuteReader();
                var results = new HashSet<string>();
                while (reader.Read())
                {
                    var id = reader.GetString(0);
                    results.Add(id);
                }
                return results;
            });

            return Task.FromResult(result.ToList());
        }

        private List<string> InternalGetAliases(string key, SqliteConnection connection, SqliteTransaction transaction)
        {
            var aliasesQuery = $@"
                SELECT id FROM {_kvsTableName}
                WHERE isAliasFor = @key";
            using var aliasesCommand = new SqliteCommand(aliasesQuery, connection, transaction);
            aliasesCommand.Parameters.AddWithValue("@key", key);
            using var aliasesReader = aliasesCommand.ExecuteReader();

            var aliases = new List<string>();
            while (aliasesReader.Read())
            {
                var id = aliasesReader["id"].ToString() ?? throw new StorageException("Unable to retrieve id from kvs table");
                aliases.Add(id);
            }

            return aliases.Distinct().ToList();
        }

        public Task<(string Value, List<string> Aliases)> GetMapping(string key)
        {
            var result = WithTransaction((connection, transaction) =>
            {
                var value = InternalGetValueFromKey(key, connection, transaction) ?? string.Empty;
                var aliases = InternalGetAliases(key, connection, transaction);
                return (value, aliases);
            });

            return Task.FromResult(result);
        }

        public Task<List<(string Key, string Value, List<string> Aliases)>> GetAllMappings()
        {
            var result = WithConnection(connection =>
            {
                var query = $"SELECT id, isAliasFor, value FROM {_kvsTableName}";
                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                var mappings = new Dictionary<string, (string Value, HashSet<string> Aliases)>();
                while (reader.Read())
                {
                    var id = reader["id"].ToString() ?? throw new StorageException("Unable to retrieve id from kvs table");
                    var isAliasFor = reader["isAliasFor"] as string;
                    var value = reader["value"] as string;

                    if (isAliasFor != null)
                    {
                        if (mappings.TryGetValue(isAliasFor, out var mapping))
                            mapping.Aliases.Add(id);
                        else
                            mappings[id] = (string.Empty, [id]);
                    }
                    else
                    {
                        if (mappings.TryGetValue(id, out var mapping))
                            mappings[id] = (value ?? mapping.Value, mapping.Aliases);
                        else
                            mappings[id] = (value ?? string.Empty, []);
                    }
                }

                return mappings
                    .Select(kvp => (kvp.Key, kvp.Value.Value, kvp.Value.Aliases.ToList()))
                    .ToList();
            });

            return Task.FromResult(result);
        }
        public Task AddMappings(List<(string Key, string Value, List<string> Aliases)> mappings, bool overwriteExisting)
        {
            WithTransaction((connection, transaction) =>
            {
                var insertOrUpdateCommand = new SqliteCommand(
                    $@"
                    INSERT INTO {_kvsTableName} (id, isAliasFor, value)
                    VALUES (@key, NULL, @value)
                    ON CONFLICT(id) DO UPDATE SET value = @value, isAliasFor = NULL;", connection, transaction);
                var insertOrUpdateKeyParameter = insertOrUpdateCommand.Parameters.Add("@key", SqliteType.Text);
                var insertOrUpdateValueParameter = insertOrUpdateCommand.Parameters.Add("@value", SqliteType.Text);

                var deleteAliasesCommand = new SqliteCommand($@"
                    DELETE FROM {_kvsTableName}
                    WHERE isAliasFor = @key;
                ", connection, transaction);
                var deleteAliasesKeyParameter = deleteAliasesCommand.Parameters.Add("@key", SqliteType.Text);

                var insertOrUpdateAliasCommand = new SqliteCommand(
                    $@"
                    INSERT INTO {_kvsTableName} (id, isAliasFor, value)
                    VALUES (@key, @isAliasFor, NULL)
                    ON CONFLICT(id) DO UPDATE SET isAliasFor = @isAliasFor, value = NULL;", connection, transaction);
                var insertOrUpdateAliasKeyParameter = insertOrUpdateAliasCommand.Parameters.AddWithValue("@key", SqliteType.Text);
                var insertOrUpdateAliasIsAliasForParameter = insertOrUpdateAliasCommand.Parameters.AddWithValue("@isAliasFor", SqliteType.Text);

                foreach (var (key, value, aliases) in mappings)
                {
                    // Insert or update the main key-value pair
                    if (overwriteExisting || InternalGetKeyAndValue(key, connection, transaction) is (null, null))
                    {
                        insertOrUpdateKeyParameter.Value = key;
                        insertOrUpdateValueParameter.Value = value;
                        insertOrUpdateCommand.ExecuteNonQuery();
                    }

                    // Handle aliases
                    foreach (var alias in aliases)
                    {
                        if (overwriteExisting || InternalGetKeyAndValue(alias, connection, transaction) is (null, null))
                        {
                            // delete aliases pointing to this alias, in case it was previously a key
                            deleteAliasesKeyParameter.Value = alias;
                            deleteAliasesCommand.ExecuteNonQuery();

                            // upsert the alias
                            insertOrUpdateAliasKeyParameter.Value = alias;
                            insertOrUpdateAliasIsAliasForParameter.Value = key;
                            insertOrUpdateAliasCommand.ExecuteNonQuery();
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
