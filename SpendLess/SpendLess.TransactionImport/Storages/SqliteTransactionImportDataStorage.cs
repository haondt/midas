using Haondt.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages;
using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Storages
{
    public class SqliteTransactionImportDataStorage : AbstractSqliteStorage, ITransactionImportDataStorage
    {
        private readonly string _tableName;
        private readonly string _transactionsTableName;
        public SqliteTransactionImportDataStorage(IOptions<SpendLessPersistenceSettings> options) : base(options)
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
                        PrimaryKey INTEGER PRIMARY KEY,
                        Value TEXT NOT NULL,
                        FOREIGN KEY (PrimaryKey) REFERENCES {_transactionsTableName}(id) ON DELETE CASCADE
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }
        public Task Set(long transactionId, TransactionImportData data)
        {
            return SetMany(new Dictionary<long, TransactionImportData>
            {
                { transactionId, data }
            });
        }

        public Task SetMany(Dictionary<long, TransactionImportData> datum)
        {

            WithTransaction((conn, trns) =>
            {
                using var command = new SqliteCommand($@"
                    INSERT INTO {_tableName} (PrimaryKey, Value)
                    VALUES (@primaryKey, @value)
                    ON CONFLICT (PrimaryKey)
                    DO UPDATE SET Value = excluded.Value
                ", conn, trns);

                var keyParameter = command.Parameters.Add("@primaryKey", SqliteType.Integer);
                var valueParameter = command.Parameters.Add("@value", SqliteType.Text);

                foreach (var (id, data) in datum)
                {
                    keyParameter.Value = id;
                    valueParameter.Value = JsonConvert.SerializeObject(data, _serializerSettings);
                    command.ExecuteNonQuery();
                }
            });

            return Task.CompletedTask;
        }

        public Task<Optional<TransactionImportData>> Get(long transactionId)
        {
            var result = WithConnection(conn =>
            {
                var query = $"SELECT Value FROM {_tableName} WHERE PrimaryKey = @key";
                using var command = new SqliteCommand(query, conn);
                command.Parameters.AddWithValue("@key", transactionId);
                return command.ExecuteScalar();
            });

            if (result == null)
                return Task.FromResult(new Optional<TransactionImportData>());
            var resultString = result.ToString()
                ?? throw new JsonException("unable to deserialize result");
            var value = JsonConvert.DeserializeObject<TransactionImportData>(resultString, _serializerSettings)
                ?? throw new JsonException("unable to deserialize result");
            return Task.FromResult(new Optional<TransactionImportData>(value));
        }
        public Task<Dictionary<long, TransactionImportData>> GetMany(List<long> transactionIds)
        {
            var result = WithConnection(conn =>
            {
                var idIsOneOfParameters = transactionIds
                    .Select((v, j) => ($"@idIsOneOf{j}", v));
                var idIsOneOfInString = string.Join(", ", idIsOneOfParameters.Select(q => q.Item1));
                var query = $"SELECT PrimaryKey, Value FROM {_tableName} WHERE PrimaryKey IN ({idIsOneOfInString})";
                using var command = new SqliteCommand(query, conn);

                foreach (var param in idIsOneOfParameters)
                {
                    command.Parameters.AddWithValue(param.Item1, param.v);
                }

                using var reader = command.ExecuteReader();
                var datum = new Dictionary<long, TransactionImportData>();
                while (reader.Read())
                {
                    var key = reader.GetInt64(0);
                    var valueString = reader.GetString(1)
                        ?? throw new JsonException($"unable to deserialize TransactionImportData {key}");
                    var data = JsonConvert.DeserializeObject<TransactionImportData>(valueString, _serializerSettings)
                        ?? throw new JsonException($"unable to deserialize TransactionImportData {key}");
                    datum[key] = data;
                }

                return datum;
            });

            return Task.FromResult(result);
        }
    }
}
