using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages;

namespace SpendLess.Transactions.Storages
{
    public class SqliteTransactionStorage : AbstractSqliteStorage, ITransactionStorage
    {
        private readonly string _tableName;
        private readonly string _tagsTableName;

        public SqliteTransactionStorage(IOptions<SpendLessPersistenceSettings> options) : base(options)
        {
            _tableName = SanitizeTableName(options.Value.TransactionsTableName);
            _tagsTableName = SanitizeTableName(options.Value.TransactionsTagsTableName);
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
                        amountCents INTEGER NOT NULL,
                        timeStamp INTEGER NOT NULL,
                        category TEXT NOT NULL,
                        sourceAccount TEXT NOT NULL,
                        destinationAccount TEXT NOT NULL,
                        description TEXT NOT NULL,
                        importAccount TEXT NOT NULL,
                        sourceData TEXT NOT NULL,
                        sourceDataHash INTEGER NOT NULL
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();

                using var createTagsTableCommand = new SqliteCommand(
                    $@"
                        CREATE TABLE {_tagsTableName} (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        tid INTEGER NOT NULL,
                        FOREIGN KEY (tid) REFERENCES {_tableName}(id) ON DELETE CASCADE,
                        UNIQUE (name, tid)
                    );", connection, transaction);
                createTagsTableCommand.ExecuteNonQuery();
            });
        }

        public Task<List<long>> AddTransactions(List<TransactionDto> transactions)
        {
            var result = WithTransaction((conn, trns) => AddTransactionsInternal(transactions, conn, trns));
            return Task.FromResult(result);
        }

        private List<long> AddTransactionsInternal(List<TransactionDto> transactions, SqliteConnection conn, SqliteTransaction trns)
        {
            using var insertCommand = new SqliteCommand(
                $@"
                    INSERT INTO {_tableName}
                    (amountCents,
                        timeStamp,
                        category,
                        sourceAccount,
                        destinationAccount,
                        description,
                        importAccount,
                        sourceData,
                        sourceDataHash)
                    VALUES
                    (@AmountCents,
                        @TimeStamp,
                        @Category,
                        @SourceAccount,
                        @DestinationAccount,
                        @Description,
                        @ImportAccount,
                        @SourceData,
                        @SourceDataHash);
                    SELECT last_insert_rowid();
                    ", conn, trns);

            var amountCentsParameter = insertCommand.Parameters.Add("@AmountCents", SqliteType.Integer);
            var timeStampParameter = insertCommand.Parameters.Add("@TimeStamp", SqliteType.Integer);
            var categoryParameter = insertCommand.Parameters.Add("@Category", SqliteType.Text);
            var sourceAccountParameter = insertCommand.Parameters.Add("@SourceAccount", SqliteType.Text);
            var destinationAccountParameter = insertCommand.Parameters.Add("@DestinationAccount", SqliteType.Text);
            var descriptionParameter = insertCommand.Parameters.Add("@Description", SqliteType.Text);
            var importAccountParameter = insertCommand.Parameters.Add("@ImportAccount", SqliteType.Text);
            var sourceDataParameter = insertCommand.Parameters.Add("@SourceData", SqliteType.Text);
            var sourceDataHashParameter = insertCommand.Parameters.Add("@SourceDataHash", SqliteType.Integer);

            using var insertTagCommand = new SqliteCommand(
                $@"
                    INSERT INTO {_tagsTableName} (name, tid)
                    VALUES (@Name, @Transaction);
                    ", conn, trns);
            var tagNameParameter = insertTagCommand.Parameters.Add("@Name", SqliteType.Text);
            var tagTransactionParameter = insertTagCommand.Parameters.Add("@Transaction", SqliteType.Integer);

            var result = new List<long>();
            foreach (var transaction in transactions)
            {
                amountCentsParameter.Value = (long)(transaction.Amount * 100);
                categoryParameter.Value = transaction.Category;
                timeStampParameter.Value = transaction.TimeStamp;
                sourceAccountParameter.Value = transaction.SourceAccount;
                destinationAccountParameter.Value = transaction.DestinationAccount;
                descriptionParameter.Value = transaction.Description;
                importAccountParameter.Value = transaction.ImportAccount;
                sourceDataParameter.Value = transaction.SourceDataString;
                sourceDataHashParameter.Value = transaction.SourceDataHash;

                var transactionId = (long)insertCommand.ExecuteScalar()!;
                tagTransactionParameter.Value = transactionId;
                foreach (var tag in transaction.Tags)
                {
                    tagNameParameter.Value = tag;
                    insertTagCommand.ExecuteNonQuery();
                }

                result.Add(transactionId);
            }

            return result;

        }

        public (StorageOperation Operation, Func<List<long>> GetResult) CreateAddTransactionsOperation(List<TransactionDto> transactions)
        {
            var result = new List<long>();
            var operation = new CustomSqliteStorageOperation
            {
                Target = StorageKey<TransactionDto>.Empty,
                Execute = (conn, trns) =>
                {
                    result = AddTransactionsInternal(transactions, conn, trns);
                }
            };

            return (operation, () => result);
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

        public Task<TransactionDto> GetTransaction(long key)
        {
            throw new NotImplementedException();
        }
    }
}
