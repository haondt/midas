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
                amountCentsParameter.Value = (int)(transaction.Amount * 100);
                categoryParameter.Value = transaction.Category;
                timeStampParameter.Value = transaction.TimeStamp;
                sourceAccountParameter.Value = transaction.SourceAccount;
                destinationAccountParameter.Value = transaction.DestinationAccount;
                descriptionParameter.Value = transaction.Description;
                importAccountParameter.Value = transaction.ImportAccount;
                sourceDataParameter.Value = transaction.SourceDataString;
                sourceDataHashParameter.Value = transaction.SourceDataHash;

                var transactionId = Convert.ToInt64(insertCommand.ExecuteScalar());
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

        private (SqliteCommand Command, Func<long, List<string>> Execute) GetGetTransactionTagsCommand(SqliteConnection conn)
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


        public Task<TransactionDto> GetTransaction(long key)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteTransaction(long key) => (await DeleteTransactions([key])) > 0;

        public Task<int> DeleteTransactions(List<long> keys)
        {
            var result = WithTransaction((conn, trns) =>
            {
                var command = new SqliteCommand($"DELETE FROM {_tableName} WHERE id = @key",
                    conn, trns);
                var parameter = command.Parameters.Add("@key", SqliteType.Integer);

                var deleted = 0;
                foreach (var key in keys)
                {
                    parameter.Value = key;
                    deleted += command.ExecuteNonQuery();
                }

                return deleted;
            });

            return Task.FromResult(result);
        }

        public Task<int> DeleteAllTransactions()
        {
            var result = WithConnection(conn =>
            {
                var command = new SqliteCommand($"DELETE FROM {_tableName}", conn);
                return command.ExecuteNonQuery();
            });

            return Task.FromResult(result);
        }

        public Task<int> DeleteTransactions(List<TransactionFilter> filters)
        {
            var baseQuery = new StringBuilder($@"
                SELECT t.id
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
            var (whereClauses, parameters) = GenerateWhereClauses(filters);

            if (whereClauses.Count > 0)
            {
                var conjunction = hasTagFilters.Any() ? "AND" : "WHERE";
                baseQuery.AppendLine($"{conjunction} {string.Join(" AND ", whereClauses)}");
            }

            // Execute query
            var result = WithTransaction((conn, trns) =>
            {
                using var command = new SqliteCommand(baseQuery.ToString(), conn, trns);
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

                for (int i = 0; i < hasTagFilters.Count; i++)
                    command.Parameters.Add(new SqliteParameter($"@Tag{i}", hasTagFilters[i].Value));

                var idList = new List<int>();
                var reader = command.ExecuteReader();
                while (reader.Read())
                    idList.Add(Convert.ToInt32(reader["id"]));

                var deleteCommand = new SqliteCommand($"DELETE FROM {_tableName} WHERE id = @key",
                    conn, trns);
                var deleteParameter = deleteCommand.Parameters.Add("@key", SqliteType.Integer);

                var deleted = 0;
                foreach (var key in idList)
                {
                    deleteParameter.Value = key;
                    deleted += deleteCommand.ExecuteNonQuery();
                }

                return Convert.ToInt32(deleted);
            });

            return Task.FromResult(result);

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
            var (whereClauses, parameters) = GenerateWhereClauses(filters);

            if (whereClauses.Count > 0)
            {
                var conjunction = hasTagFilters.Any() ? "AND" : "WHERE";
                baseQuery.AppendLine($"{conjunction} {string.Join(" AND ", whereClauses)}");
            }

            baseQuery.AppendLine($"GROUP BY {groupBy}");

            // Execute query
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

        public Task<Dictionary<long, TransactionDto>> GetTransactions(List<TransactionFilter> filters, long? limit = null, long? offset = null)
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
            var (whereClauses, parameters) = GenerateWhereClauses(filters);

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

                var transactions = new Dictionary<long, TransactionDto>();
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
                    transactions.Add(Convert.ToInt64(reader["id"]), transaction);
                }

                return transactions;
            });

            return Task.FromResult(result);
        }

        public Task<long> GetTransactionsCount(List<TransactionFilter> filters)
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
            var (whereClauses, parameters) = GenerateWhereClauses(filters);

            if (whereClauses.Count > 0)
            {
                var conjunction = hasTagFilters.Any() ? "AND" : "WHERE";
                baseQuery.AppendLine($"{conjunction} {string.Join(" AND ", whereClauses)}");
            }

            // Execute query
            var result = WithConnection(conn =>
            {
                using var command = new SqliteCommand(baseQuery.ToString(), conn);
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

                for (int i = 0; i < hasTagFilters.Count; i++)
                    command.Parameters.Add(new SqliteParameter($"@Tag{i}", hasTagFilters[i].Value));

                var transactions = new Dictionary<int, TransactionDto>();
                var result = command.ExecuteScalar();
                return Convert.ToInt64(result);
            });

            return Task.FromResult(result);
        }

        private (List<string> WhereClauses, List<SqliteParameter> Parameters) GenerateWhereClauses(List<TransactionFilter> filters)
        {
            var whereClauses = new List<string>();
            var parameters = new List<SqliteParameter>();

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                switch (filter)
                {
                    case MinDateTransactionFilter minDateFilter:
                        whereClauses.Add($"t.timeStamp >= @MinDate{i}");
                        parameters.Add(new SqliteParameter($"@MinDate{i}", minDateFilter.UnixSeconds));
                        break;
                    case MaxDateTransactionFilter maxDateFilter:
                        whereClauses.Add($"t.timeStamp <= @MaxDate{i}");
                        parameters.Add(new SqliteParameter($"@MaxDate{i}", maxDateFilter.UnixSeconds));
                        break;
                    case ExclusiveMaxDateTransactionFilter maxDateFilter:
                        whereClauses.Add($"t.timeStamp < @ExclusiveMaxDate{i}");
                        parameters.Add(new SqliteParameter($"@ExclusiveMaxDate{i}", maxDateFilter.UnixSeconds));
                        break;
                    case MinAmountTransactionFilter minAmountFilter:
                        whereClauses.Add($"t.amountCents >= @MinAmountCents{i}");
                        parameters.Add(new SqliteParameter($"@MinAmountCents{i}", minAmountFilter.AmountCents));
                        break;
                    case MaxAmountTransactionFilter maxAmountFilter:
                        whereClauses.Add($"t.amountCents <= @MaxAmountCents{i}");
                        parameters.Add(new SqliteParameter($"@MaxAmountCents{i}", maxAmountFilter.AmountCents));
                        break;
                    case HasAmountTransactionFilter hasAmountFilter:
                        whereClauses.Add($"t.amountCents = @HasAmountCents{i}");
                        parameters.Add(new SqliteParameter($"@HasAmountCents{i}", hasAmountFilter.AmountCents));
                        break;
                    case HasCategoryTransactionFilter hasCategoryFilter:
                        whereClauses.Add($"t.category = @Category{i}");
                        parameters.Add(new SqliteParameter($"@Category{i}", hasCategoryFilter.Value));
                        break;
                    case EitherAccountIsTransactionFilter eitherAccountFilter:
                        whereClauses.Add($"(t.sourceAccount = @EitherAccountId{i} OR t.destinationAccount = @EitherAccountId{i})");
                        parameters.Add(new SqliteParameter($"@EitherAccountId{i}", eitherAccountFilter.Id));
                        break;
                    case EitherAccountIsOneOfTransactionFilter eitherAccountOneOfFilter:
                        var eitherAccountOneOfParameters = eitherAccountOneOfFilter.Ids
                            .Select((v, j) => ($"@EitherAccountOneOf{i}_{j}", v));
                        var eitherAccountOneOfInString = string.Join(", ", eitherAccountOneOfParameters.Select(q => q.Item1));
                        whereClauses.Add($"(t.sourceAccount IN ({eitherAccountOneOfInString}) OR t.destinationAccount IN ({eitherAccountOneOfInString}))");
                        parameters.AddRange(eitherAccountOneOfParameters.Select(q => new SqliteParameter(q.Item1, q.Item2)));
                        break;

                    case DescriptionContainsTransactionFilter descriptionContainsFilter:
                        whereClauses.Add($"t.description LIKE @DescriptionContains{i}");
                        parameters.Add(new SqliteParameter($"@DescriptionContains{i}", SqliteParameterCollectionExtensions.PrepareSqliteLikeTerm(descriptionContainsFilter.Value)));
                        break;
                    case HasTagTransactionFilter:
                        break; // handled seperately
                    default:
                        throw new NotSupportedException($"Transaction filter of type {filter.GetType()} is not supported.");
                }
            }

            return (whereClauses, parameters);
        }

        public Task<List<string>> GetCategories()
        {
            var result = WithConnection(conn =>
            {
                using var command = new SqliteCommand(
                    $@"
                        SELECT DISTINCT category
                        FROM {_tableName}
                        ORDER BY category ASC;
                    ", conn);

                var categories = new List<string>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var category = reader["category"].ToString();
                    if (!string.IsNullOrEmpty(category))
                        categories.Add(category);
                }
                return categories;
            });

            return Task.FromResult(result);
        }

        public Task<List<string>> GetTags()
        {
            var result = WithConnection(conn =>
            {
                using var command = new SqliteCommand(
                    $@"
                        SELECT DISTINCT name as tag
                        FROM {_tagsTableName}
                        ORDER BY tag ASC;
                    ", conn);

                var tags = new List<string>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var tag = reader["tag"].ToString();
                    if (!string.IsNullOrEmpty(tag))
                        tags.Add(tag);
                }
                return tags;
            });

            return Task.FromResult(result);
        }
    }
}
