﻿using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
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

            WithTransaction((connection, transaaction) =>
            {
                using var createPrimaryTableCommand = new SqliteCommand(
                    $@"
                        CREATE TABLE {_kvsTableName} (
                        key TEXT PRIMARY KEY
                     );", connection, transaaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }

        public Task AddKey(string key)
        {
            WithTransaction((connection, transcation) =>
            {
                using var insertCommand = new SqliteCommand(
                    $@"
                        INSERT OR REPLACE INTO {_kvsTableName} (key)
                        VALUES (@key);", connection, transcation);
                insertCommand.Parameters.AddWithValue("@key", key);
                insertCommand.ExecuteNonQuery();
            });

            return Task.CompletedTask;
        }

        public Task RemoveKey(string key)
        {
            WithTransaction(async (connection, transaction) =>
            {
                using var deleteCommand = new SqliteCommand(
                    $@"
                        DELETE FROM {_kvsTableName}
                        WHERE key = @key;", connection, transaction);

                deleteCommand.Parameters.AddWithValue("@key", key);
                await deleteCommand.ExecuteNonQueryAsync();
            });

            return Task.CompletedTask;
        }


        public Task<List<string>> SearchKey(string partialKey)
        {
            var result = WithConnection(connection =>
            {
                List<string> matchingKeys = [];
                using var searchCommand = new SqliteCommand(
                    $@"
                        SELECT key
                        FROM {_kvsTableName}
                        WHERE key LIKE @partialKey
                        LIMIT {_settings.MaxKvsSearchHits};", connection);

                searchCommand.Parameters.AddLikeTermWithValue("@partialKey", partialKey);

                using var reader = searchCommand.ExecuteReader();
                while (reader.Read())
                {
                    matchingKeys.Add(reader.GetString(0));
                }
                return matchingKeys;
            });

            return Task.FromResult(result);
        }

        public Task<List<string>> GetAllKeys()
        {
            var result = WithConnection(connection =>
            {
                var keys = new List<string>();
                using var selectCommand = new SqliteCommand($"SELECT key FROM {_kvsTableName};", connection);
                using var reader = selectCommand.ExecuteReader();
                while (reader.Read())
                    keys.Add(reader.GetString(0));

                return keys;
            });
            return Task.FromResult(result);
        }
    }
}