using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Persistence.Storages;
using SpendLess.Transactions.Models;
using System.Text;

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

        public Task<List<int>> AddTransactions(List<TransactionDto> transactions)
        {
            var result = WithTransaction((conn, trns) => AddTransactionsInternal(transactions, conn, trns));
            return Task.FromResult(result);
        }

        private List<int> AddTransactionsInternal(List<TransactionDto> transactions, SqliteConnection conn, SqliteTransaction trns)
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

            var result = new List<int>();
            foreach (var transaction in transactions)
            {
                amountCentsParameter.Value = (int)(transaction.Amount * 100);
                categoryParameter.Value = transaction.Category;
                timeStampParameter.Value = transaction.TimeStamp;
                sourceAccountParameter.Value = transaction.SourceAccount;
                destinationAccountParameter.Value = transaction.DestinationAccount;
                descriptionParameter.Value = transaction.Description;
                importAccountParameter.Value = transaction.ImportAccount;
                sourceDataParameter.Value = transaction.SourceDataString;
                sourceDataHashParameter.Value = transaction.SourceDataHash;

                var transactionId = (int)insertCommand.ExecuteScalar()!;
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

        public (StorageOperation Operation, Func<List<int>> GetResult) CreateAddTransactionsOperation(List<TransactionDto> transactions)
        {
            var result = new List<int>();
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

        private (SqliteCommand Command, Func<int, List<string>> Execute) GetGetTransactionTagsCommand(SqliteConnection conn)
        {
            var command = new SqliteCommand(
                 $@"
                SELECT name 
                FROM {_tagsTableName}
                WHERE tid = @TransactionId;
            ", conn);

            var parameter = command.Parameters.Add("@TransactionId", SqliteType.Integer);

            return (command, id =>
            {
                parameter.Value = id;
                var tags = new List<string>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    tags.Add(reader["name"].ToString()!);
                return tags;
            }
            );
        }


        public Task<TransactionDto> GetTransaction(int key)
        {
            throw new NotImplementedException();
        }

        public Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters)
        {
            var result = WithConnection(conn => (
                InternalGetAmounts(conn, filters, "sourceAccount"),
                InternalGetAmounts(conn, filters, "destinationAccount")));
            return Task.FromResult(result);
        }

        private Dictionary<string, decimal> InternalGetAmounts(SqliteConnection conn, List<TransactionFilter> filters, string groupBy)
        {
            var baseQuery = new StringBuilder($@"
                SELECT SUM(t.amountCents) AS amount, t.{groupBy} AS grouping
                FROM {_tableName} t
            ");

            // Handle tag filters with INTERSECT
            var hasTagFilters = filters.OfType<HasTagTransactionFilter>().ToList();
            if (hasTagFilters.Count > 0)
            {
                var tagQueries = hasTagFilters.Select((filter, index) =>
                    $@"
                        SELECT tid 
                        FROM {_tagsTableName} 
                        WHERE name = @Tag{index}
                    ");

                baseQuery.AppendLine("WHERE t.id IN (")
                         .AppendLine(string.Join("\nINTERSECT\n", tagQueries))
                         .AppendLine(")");
            }

            // Add WHERE clause for other filters
            var whereClauses = new List<string>();
            var parameters = new List<SqliteParameter>();

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                switch (filter)
                {
                    case MinDateTransactionFilter minDateFilter:
                        whereClauses.Add("t.timeStamp >= @MinDate");
                        parameters.Add(new SqliteParameter("@MinDate", minDateFilter.UnixSeconds));
                        break;
                    case MaxDateTransactionFilter maxDateFilter:
                        whereClauses.Add("t.timeStamp <= @MaxDate");
                        parameters.Add(new SqliteParameter("@MaxDate", maxDateFilter.UnixSeconds));
                        break;
                    case MinAmountTransactionFilter minAmountFilter:
                        whereClauses.Add("t.amountCents >= @MinAmountCents");
                        parameters.Add(new SqliteParameter("@MinAmountCents", minAmountFilter.AmountCents));
                        break;
                    case HasCategoryTransactionFilter hasCategoryFilter:
                        whereClauses.Add("t.category = @Category");
                        parameters.Add(new SqliteParameter("@Category", hasCategoryFilter.Value));
                        break;
                    case EitherAccountIsTransactionFilter eitherAccountFilter:
                        whereClauses.Add($"(t.sourceAccount = @EitherAccountId{i} OR t.destinationAccount = @EitherAccountId{i})");
                        parameters.Add(new SqliteParameter($"@EitherAccountId{i}", eitherAccountFilter.Id));
                        break;
                    default:
                        throw new NotSupportedException($"Transaction filter of type {filter.GetType()} is not supported.");
                }
            }

            if (whereClauses.Count > 0)
            {
                var conjunction = hasTagFilters.Any() ? "AND" : "WHERE";
                baseQuery.AppendLine($"{conjunction} {string.Join(" AND ", whereClauses)}");
            }

            baseQuery.AppendLine($"GROUP BY {groupBy}");

            // Execute query
            var getGetTags = GetGetTransactionTagsCommand(conn);
            using var getTagsCommand = getGetTags.Command;

            using var command = new SqliteCommand(baseQuery.ToString(), conn);
            foreach (var parameter in parameters)
                command.Parameters.Add(parameter);

            for (int i = 0; i < hasTagFilters.Count; i++)
                command.Parameters.Add(new SqliteParameter($"@Tag{i}", hasTagFilters[i].Value));

            Dictionary<string, decimal> results = [];
            using var reader = command.ExecuteReader();
            while (reader.Read())
                results.Add(reader["grouping"].ToString() ?? throw new NullReferenceException(), Convert.ToInt32(reader["amount"]) / 100m);

            return results;
        }

        public Task<Dictionary<int, TransactionDto>> GetTransactions(List<TransactionFilter> filters, int? limit = null, int? offset = null)
        {
            var baseQuery = new StringBuilder($@"
                SELECT 
                    t.id,
                    t.amountCents,
                    t.timeStamp,
                    t.category,
                    t.sourceAccount,
                    t.destinationAccount,
                    t.description,
                    t.importAccount,
                    t.sourceData,
                    t.sourceDataHash
                FROM {_tableName} t
            ");

            // Handle tag filters with INTERSECT
            var hasTagFilters = filters.OfType<HasTagTransactionFilter>().ToList();
            if (hasTagFilters.Count > 0)
            {
                var tagQueries = hasTagFilters.Select((filter, index) =>
                    $@"
                        SELECT tid 
                        FROM {_tagsTableName} 
                        WHERE name = @Tag{index}
                    ");

                baseQuery.AppendLine("WHERE t.id IN (")
                         .AppendLine(string.Join("\nINTERSECT\n", tagQueries))
                         .AppendLine(")");
            }

            // Add WHERE clause for other filters
            var whereClauses = new List<string>();
            var parameters = new List<SqliteParameter>();

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                switch (filter)
                {
                    case MinDateTransactionFilter minDateFilter:
                        whereClauses.Add("t.timeStamp >= @MinDate");
                        parameters.Add(new SqliteParameter("@MinDate", minDateFilter.UnixSeconds));
                        break;
                    case MaxDateTransactionFilter maxDateFilter:
                        whereClauses.Add("t.timeStamp <= @MaxDate");
                        parameters.Add(new SqliteParameter("@MaxDate", maxDateFilter.UnixSeconds));
                        break;
                    case MinAmountTransactionFilter minAmountFilter:
                        whereClauses.Add("t.amountCents >= @MinAmountCents");
                        parameters.Add(new SqliteParameter("@MinAmountCents", minAmountFilter.AmountCents));
                        break;
                    case HasCategoryTransactionFilter hasCategoryFilter:
                        whereClauses.Add("t.category = @Category");
                        parameters.Add(new SqliteParameter("@Category", hasCategoryFilter.Value));
                        break;
                    case EitherAccountIsTransactionFilter eitherAccountFilter:
                        whereClauses.Add($"(t.sourceAccount = @EitherAccountId{i} OR t.destinationAccount = @EitherAccountId{i})");
                        parameters.Add(new SqliteParameter($"@EitherAccountId{i}", eitherAccountFilter.Id));
                        break;
                    case DescriptionContainsTransactionFilter descriptionContainsFilter:
                        whereClauses.Add($"t.description LIKE @DescriptionContains{i}");
                        parameters.Add(new SqliteParameter($"@DescriptionContains{i}", SqliteParameterCollectionExtensions.PrepareSqliteLikeTerm(descriptionContainsFilter.Value)));
                        break;
                    default:
                        throw new NotSupportedException($"Transaction filter of type {filter.GetType()} is not supported.");
                }
            }

            if (whereClauses.Count > 0)
            {
                var conjunction = hasTagFilters.Any() ? "AND" : "WHERE";
                baseQuery.AppendLine($"{conjunction} {string.Join(" AND ", whereClauses)}");
            }

            baseQuery.AppendLine("ORDER BY t.timeStamp DESC");
            if (limit.HasValue)
                baseQuery.AppendLine($"LIMIT {limit.Value}");
            if (offset.HasValue)
                baseQuery.AppendLine($"OFFSET {offset.Value}");

            // Execute query
            var result = WithConnection(conn =>
            {
                var getGetTags = GetGetTransactionTagsCommand(conn);
                using var getTagsCommand = getGetTags.Command;

                using var command = new SqliteCommand(baseQuery.ToString(), conn);
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

                for (int i = 0; i < hasTagFilters.Count; i++)
                    command.Parameters.Add(new SqliteParameter($"@Tag{i}", hasTagFilters[i].Value));

                var transactions = new Dictionary<int, TransactionDto>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var transaction = new TransactionDto
                    {
                        Amount = Convert.ToDecimal(reader["amountCents"]) / 100,
                        TimeStamp = Convert.ToInt64(reader["timeStamp"]),
                        Category = reader["category"].ToString() ?? "",
                        SourceAccount = reader["sourceAccount"].ToString() ?? "",
                        DestinationAccount = reader["destinationAccount"].ToString() ?? "",
                        Description = reader["description"].ToString() ?? "",
                        ImportAccount = reader["importAccount"].ToString() ?? "",
                        SourceData = TransactionDto.DestringifySourceData(reader["sourceData"].ToString() ?? ""),
                        Tags = getGetTags.Execute(Convert.ToInt32(reader["id"])).ToHashSet()
                    };
                    transactions.Add(Convert.ToInt32(reader["id"]), transaction);
                }

                return transactions;
            });

            return Task.FromResult(result);
        }

        public Task<int> GetTransactionsCount(List<TransactionFilter> filters)
        {
            var baseQuery = new StringBuilder($@"
                SELECT COUNT(*) as totalRecords
                FROM {_tableName} t
            ");

            // Handle tag filters with INTERSECT
            var hasTagFilters = filters.OfType<HasTagTransactionFilter>().ToList();
            if (hasTagFilters.Count > 0)
            {
                var tagQueries = hasTagFilters.Select((filter, index) =>
                    $@"
                        SELECT tid 
                        FROM {_tagsTableName} 
                        WHERE name = @Tag{index}
                    ");

                baseQuery.AppendLine("WHERE t.id IN (")
                         .AppendLine(string.Join("\nINTERSECT\n", tagQueries))
                         .AppendLine(")");
            }

            // Add WHERE clause for other filters
            var whereClauses = new List<string>();
            var parameters = new List<SqliteParameter>();

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                switch (filter)
                {
                    case MinDateTransactionFilter minDateFilter:
                        whereClauses.Add("t.timeStamp >= @MinDate");
                        parameters.Add(new SqliteParameter("@MinDate", minDateFilter.UnixSeconds));
                        break;
                    case MaxDateTransactionFilter maxDateFilter:
                        whereClauses.Add("t.timeStamp <= @MaxDate");
                        parameters.Add(new SqliteParameter("@MaxDate", maxDateFilter.UnixSeconds));
                        break;
                    case MinAmountTransactionFilter minAmountFilter:
                        whereClauses.Add("t.amountCents >= @MinAmountCents");
                        parameters.Add(new SqliteParameter("@MinAmountCents", minAmountFilter.AmountCents));
                        break;
                    case HasCategoryTransactionFilter hasCategoryFilter:
                        whereClauses.Add("t.category = @Category");
                        parameters.Add(new SqliteParameter("@Category", hasCategoryFilter.Value));
                        break;
                    case EitherAccountIsTransactionFilter eitherAccountFilter:
                        whereClauses.Add($"(t.sourceAccount = @EitherAccountId{i} OR t.destinationAccount = @EitherAccountId{i})");
                        parameters.Add(new SqliteParameter($"@EitherAccountId{i}", eitherAccountFilter.Id));
                        break;
                    case DescriptionContainsTransactionFilter descriptionContainsFilter:
                        whereClauses.Add($"t.description LIKE @DescriptionContains{i}");
                        parameters.Add(new SqliteParameter($"@DescriptionContains{i}", SqliteParameterCollectionExtensions.PrepareSqliteLikeTerm(descriptionContainsFilter.Value)));
                        break;
                    default:
                        throw new NotSupportedException($"Transaction filter of type {filter.GetType()} is not supported.");
                }
            }

            if (whereClauses.Count > 0)
            {
                var conjunction = hasTagFilters.Any() ? "AND" : "WHERE";
                baseQuery.AppendLine($"{conjunction} {string.Join(" AND ", whereClauses)}");
            }

            // Execute query
            var result = WithConnection(conn =>
            {
                var getGetTags = GetGetTransactionTagsCommand(conn);
                using var getTagsCommand = getGetTags.Command;

                using var command = new SqliteCommand(baseQuery.ToString(), conn);
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

                for (int i = 0; i < hasTagFilters.Count; i++)
                    command.Parameters.Add(new SqliteParameter($"@Tag{i}", hasTagFilters[i].Value));

                var transactions = new Dictionary<int, TransactionDto>();
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            });

            return Task.FromResult(result);
        }
    }
}
