using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Persistence.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SpendLess.Core.Extensions;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Models;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages.Abstractions;

namespace SpendLess.Persistence.Storages.Sqlite
{
    public class SqliteAccountStorage : AbstractSqliteStorage, IAccountStorage
    {
        private readonly string _tableName;
        public SqliteAccountStorage(IOptions<SpendLessPersistenceSettings> options) : base(options)
        {

            _tableName = SanitizeTableName(options.Value.AccountsTableName);
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
                        id TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        isMine INTEGER NOT NULL
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }
        public Task Add(string accountId, AccountDto account)
        {
            return AddMany([(accountId, account)]);
        }

        public Task AddMany(List<(string AccountId, AccountDto Account)> values)
        {
            WithTransaction((c, t) => SetManyInternal(values, false, c, t));
            return Task.CompletedTask;
        }

        public Task Set(string accountId, AccountDto account)
        {
            return SetMany([(accountId, account)]);
        }

        public Task SetMany(List<(string AccountId, AccountDto Account)> values)
        {
            WithTransaction((c, t) => SetManyInternal(values, true, c, t));
            return Task.CompletedTask;
        }

        public void SetManyInternal(List<(string AccountId, AccountDto Account)> values, bool isUpsert, SqliteConnection conn, SqliteTransaction trns)
        {
            var columns = new List<(string Column, string Parameter, SqliteType Type, Func<(string AccountId, AccountDto Account), object> Extractor)>
                {
                    ("id", "@id", SqliteType.Text, x => x.AccountId),
                    ("name", "@name", SqliteType.Text, x => x.Account.Name),
                    ("isMine", "@isMine", SqliteType.Integer, x => x.Account.IsMine ? 1 : 0),
                };

            var query = $@"
                    INSERT INTO {_tableName} ({string.Join(", ", columns.Select(c => c.Column))})
                    VALUES ({string.Join(", ", columns.Select(c => c.Parameter))})
                ";
            if (isUpsert)
                query += $@"
                ON CONFLICT(id) DO UPDATE SET 
                    {string.Join(", ", columns.Skip(1).Select(c => $"{c.Column} = excluded.{c.Column}"))}";

            using var command = new SqliteCommand(query, conn, trns);
            var setParametersList = columns
                .Select<(string Column, string Parameter, SqliteType Type, Func<(string, AccountDto), object> Extractor), Action<(string, AccountDto)>>(c =>
                {
                    var parameter = command.Parameters.Add(c.Parameter, c.Type);
                    return d => parameter.Value = c.Extractor(d);
                }).ToList();
            void setParameters(string accountId, AccountDto account)
            {
                foreach (var set in setParametersList)
                    set((accountId, account));
            }

            foreach (var (accountId, account) in values)
            {
                setParameters(accountId, account);
                command.ExecuteNonQuery();
            }
        }


        public async Task<AccountDto> Get(string id)
        {
            var d = await GetMany([id]);
            return d.GetValue(id).Or(() => throw new StorageException($"Couldn't find account with id {id}"));
        }

        public async Task<Optional<AccountDto>> TryGet(string id)
        {
            var d = await GetMany([id]);
            return d.GetValue(id);
        }

        public Task<Dictionary<string, AccountDto>> GetMany(List<string> ids, long? limit = null, long? offset = null)
        {
            var result = WithConnection(conn =>
            {
                var idIsOneOfParameters = ids
                    .Select((v, j) => ($"@idIsOneOf{j}", v));
                var idIsOneOfInString = string.Join(", ", idIsOneOfParameters.Select(q => q.Item1));
                var query = @$"
                    SELECT id, name, isMine FROM {_tableName}
                    WHERE id IN ({idIsOneOfInString})
                    ORDER BY id DESC";

                if (limit.HasValue)
                    query += $"\nLIMIT {limit.Value}";
                if (offset.HasValue)
                    query += $"\nOFFSET {offset.Value}";
                using var command = new SqliteCommand(query, conn);
                foreach (var param in idIsOneOfParameters)
                    command.Parameters.AddWithValue(param.Item1, param.v);

                using var reader = command.ExecuteReader();
                var datum = new Dictionary<string, AccountDto>();
                while (reader.Read())
                {
                    var id = reader.GetString(0);
                    var data = new AccountDto
                    {
                        Name = reader.GetString(1),
                        IsMine = reader.GetInt16(2) > 0
                    };
                    datum[id] = data;
                }

                return datum;
            });

            return Task.FromResult(result);
        }

        public Task<Dictionary<string, AccountDto>> GetAll(long? limit = null, long? offset = null)
        {
            var result = WithConnection(conn =>
            {
                var query = @$"
                    SELECT id, name, isMine FROM {_tableName}
                    ORDER BY id DESC";

                if (limit.HasValue)
                    query += $"\nLIMIT {limit.Value}";
                if (offset.HasValue)
                    query += $"\nOFFSET {offset.Value}";
                using var command = new SqliteCommand(query, conn);

                using var reader = command.ExecuteReader();
                var datum = new Dictionary<string, AccountDto>();
                while (reader.Read())
                {
                    var id = reader.GetString(0);
                    var data = new AccountDto
                    {
                        Name = reader.GetString(1),
                        IsMine = reader.GetInt16(2) > 0
                    };
                    datum[id] = data;
                }

                return datum;
            });

            return Task.FromResult(result);
        }
        public Task<List<string>> GetAccountIdsByName(string name)
        {
            var result = WithConnection(connection =>
            {
                List<string> ids = [];
                using var searchCommand = new SqliteCommand(
                    $@"
                        SELECT id
                        FROM {_tableName}
                        WHERE name = @name", connection);

                searchCommand.Parameters.AddWithValue("@name", name);

                using var reader = searchCommand.ExecuteReader();
                while (reader.Read())
                    ids.Add(reader.GetString(0));
                return ids;
            });

            return Task.FromResult(result);
        }

        public Task<List<(string Name, string Id)>> SearchAccountsByName(string partialName, int limit)
        {
            var result = WithConnection(connection =>
            {
                List<(string Name, string Id)> matchingKeys = [];
                using var searchCommand = new SqliteCommand(
                    $@"
                        SELECT name, id
                        FROM {_tableName}
                        WHERE name LIKE @partialName
                        LIMIT {limit};", connection);

                searchCommand.Parameters.AddLikeTermWithValue("@partialName", partialName);

                using var reader = searchCommand.ExecuteReader();
                while (reader.Read())
                    matchingKeys.Add((
                        reader.GetString(0),
                        reader.GetString(1)));
                return matchingKeys;
            });

            return Task.FromResult(result);
        }

        public Task<bool> HasAccountWithName(string name)
        {
            var result = WithConnection(conn =>
            {
                var query = $"SELECT COUNT(1) FROM {_tableName} WHERE name = @name";
                using var command = new SqliteCommand(query, conn);
                command.Parameters.AddWithValue("@name", name);

                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            });

            return Task.FromResult(result);
        }

        public Task<Dictionary<string, AccountDto>> GetAllMine()
        {
            var result = WithConnection(conn =>
            {
                var query = @$"
                    SELECT id, name, isMine FROM {_tableName}
                    WHERE isMine = 1";

                using var command = new SqliteCommand(query, conn);

                using var reader = command.ExecuteReader();
                var datum = new Dictionary<string, AccountDto>();
                while (reader.Read())
                {
                    var id = reader.GetString(0);
                    var data = new AccountDto
                    {
                        Name = reader.GetString(1),
                        IsMine = reader.GetInt16(2) > 0
                    };
                    datum[id] = data;
                }

                return datum;
            });

            return Task.FromResult(result);
        }

        public async Task<bool> Delete(string id)
        {
            return await Delete([id]) > 0;
        }

        public Task<int> Delete(List<string> ids)
        {
            var result = WithTransaction((conn, trns) =>
            {
                var command = new SqliteCommand($"DELETE FROM {_tableName} WHERE id = @id", conn, trns);
                var parameter = command.Parameters.Add("@id", SqliteType.Text);
                var deleted = 0;
                foreach (var id in ids)
                {
                    parameter.Value = id;
                    deleted += command.ExecuteNonQuery();
                }
                return deleted;
            });
            return Task.FromResult(result);
        }

        public Task<int> DeleteAll()
        {
            var result = WithTransaction((conn, trns) =>
            {
                var command = new SqliteCommand($"DELETE FROM {_tableName}", conn, trns);
                return command.ExecuteNonQuery();
            });
            return Task.FromResult(result);
        }

        public Task<long> GetCount()
        {
            var result = WithTransaction((conn, trns) =>
            {
                var command = new SqliteCommand($"SELECT COUNT(1) FROM {_tableName}", conn, trns);
                return Convert.ToInt64(command.ExecuteScalar());
            });
            return Task.FromResult(result);
        }
    }
}
