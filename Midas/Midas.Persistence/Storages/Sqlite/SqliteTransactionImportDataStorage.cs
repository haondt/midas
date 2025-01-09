using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Midas.Persistence.Models;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Persistence.Storages.Sqlite
{
    public class SqliteTransactionImportDataStorage : AbstractSqliteStorage, ITransactionImportDataStorage
    {
        private readonly string _tableName;
        private readonly string _transactionsTableName;
        public SqliteTransactionImportDataStorage(IOptions<MidasPersistenceSettings> options) : base(options)
        {
            _tableName = SanitizeTableName(options.Value.TransactionsImportDataTableName);
            _transactionsTableName = SanitizeTableName(options.Value.TransactionsTableName);
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
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        transactionId INTEGER NOT NULL,
                        account TEXT NOT NULL,
                        configurationSlug TEXT NOT NULL,
                        sourceData TEXT NOT NULL,
                        sourceDataHash INTEGER NOT NULL,
                        FOREIGN KEY (transactionId) REFERENCES {_transactionsTableName}(id) ON DELETE CASCADE
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }

        public StorageOperation CreateAddManyOperation(List<TransactionImportDataDto> datum)
        {
            return new CustomSqliteStorageOperation
            {
                Target = StorageKey<TransactionImportDataDto>.Empty,
                Execute = (conn, trns) =>
                {
                    AddManyInternal(datum, conn, trns);
                }
            };
        }
        public Task<List<bool>> CheckIfHasSourceDataHash(IEnumerable<long> hashes)
        {
            var result = WithConnection(conn =>
            {
                using var command = new SqliteCommand(
                    $@"
                    WITH hashes(hash) AS (VALUES {string.Join(',', hashes.Select(h => $"({h})"))})

                    SELECT hash,
                        CASE WHEN hash IN (SELECT sourceDataHash FROM {_tableName}) THEN 1 ELSE 0 END AS isDuplicate
                    
                    FROM hashes;
                    ", conn);

                var hasHash = new Dictionary<long, bool>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var hash = Convert.ToInt64(reader["hash"]);
                    var isDuplicate = Convert.ToBoolean(reader["isDuplicate"]);
                    hasHash[hash] = isDuplicate;
                }

                return hasHash;
            });

            return Task.FromResult(hashes.Select(h => result[h]).ToList());
        }

        public Task Add(TransactionImportDataDto data) => AddMany([data]);

        public Task AddMany(IEnumerable<TransactionImportDataDto> datum)
        {
            WithTransaction((conn, trns) =>
            {
                AddManyInternal(datum, conn, trns);
            });
            return Task.CompletedTask;
        }

        private void AddManyInternal(IEnumerable<TransactionImportDataDto> datum, SqliteConnection conn, SqliteTransaction trns)
        {
            var columns = new List<(string Column, string Parameter, SqliteType Type, Func<TransactionImportDataDto, object> Extractor)>
                {
                    ("transactionId", "@transactionId", SqliteType.Integer, x => x.Transaction),
                    ("account", "@account", SqliteType.Text, x => x.Account),
                    ("configurationSlug", "@configurationSlug", SqliteType.Text, x => x.ConfigurationSlug),
                    ("sourceData", "@sourceData", SqliteType.Text, x => x.SourceDataString),
                    ("sourceDataHash", "@sourceDataHash", SqliteType.Integer, x => x.SourceDataHash),
                };

            using var command = new SqliteCommand($@"
                    INSERT INTO {_tableName} ({string.Join(", ", columns.Select(c => c.Column))})
                    VALUES ({string.Join(", ", columns.Select(c => c.Parameter))})
                ", conn, trns);

            var setParametersList = columns
                .Select<(string Column, string Parameter, SqliteType Type, Func<TransactionImportDataDto, object> Extractor), Action<TransactionImportDataDto>>(c =>
                {
                    var parameter = command.Parameters.Add(c.Parameter, c.Type);
                    return d => parameter.Value = c.Extractor(d);
                }).ToList();
            void setParameters(TransactionImportDataDto data)
            {
                foreach (var set in setParametersList)
                    set(data);
            }

            foreach (var data in datum)
            {
                setParameters(data);
                command.ExecuteNonQuery();
            }
        }

        public Task<List<List<TransactionImportDataDto>>> GetMany(IEnumerable<long> transactionIds)
        {
            var result = WithConnection(conn =>
            {
                var idIsOneOfParameters = transactionIds
                    .Select((v, j) => ($"@idIsOneOf{j}", v));
                var idIsOneOfInString = string.Join(", ", idIsOneOfParameters.Select(q => q.Item1));
                var query = $"SELECT transactionId, account, configurationSlug, sourceData, sourceDataHash FROM {_tableName} WHERE transactionId IN ({idIsOneOfInString})";
                using var command = new SqliteCommand(query, conn);
                foreach (var param in idIsOneOfParameters)
                    command.Parameters.AddWithValue(param.Item1, param.v);

                using var reader = command.ExecuteReader();
                var datum = new Dictionary<long, List<TransactionImportDataDto>>();
                while (reader.Read())
                {
                    var data = new TransactionImportDataDto
                    {
                        Transaction = reader.GetInt64(0),
                        Account = reader.GetString(1),
                        ConfigurationSlug = reader.GetString(2),
                        SourceData = TransactionImportDataDto.DestringifySourceData(reader.GetString(3))
                    };
                    if (!datum.TryGetValue(data.Transaction, out var list))
                        list = datum[data.Transaction] = [];
                    list.Add(data);
                }

                return datum;
            });

            return Task.FromResult(transactionIds.Select(x => result.GetValueOrDefault(x, [])).ToList());
        }


        public async Task<List<TransactionImportDataDto>> Get(long transactionId)
        {
            return (await GetMany([transactionId]))[0];
        }

    }
}
