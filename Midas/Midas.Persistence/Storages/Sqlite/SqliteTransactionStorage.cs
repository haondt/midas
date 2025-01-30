using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Persistence.Sqlite.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Midas.Core.Extensions;
using Midas.Core.Models;
using Midas.Persistence.Extensions;
using Midas.Persistence.Models;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;
using System.Text;

namespace Midas.Persistence.Storages.Sqlite
{
    public class SqliteTransactionStorage : AbstractSqliteStorage, ITransactionStorage
    {
        private readonly string _tableName;
        private readonly string _tagsTableName;
        private readonly string _supercategoriesTableName;
        private const string _supercategoriesTableAlias = "c";
        private readonly string _accountsTableName;
        private readonly string _importDataTableName;
        private const string _importDataTableAlias = "idt";
        private const string _sourceAccountTableAlias = "s";
        private const string _destinationAccountTableAlias = "d";

        public SqliteTransactionStorage(IOptions<MidasPersistenceSettings> options) : base(options)
        {
            _tableName = SanitizeTableName(options.Value.TransactionsTableName);
            _tagsTableName = SanitizeTableName(options.Value.TransactionsTagsTableName);
            _supercategoriesTableName = SanitizeTableName(options.Value.SupercategoryTableName);
            _accountsTableName = SanitizeTableName(options.Value.AccountsTableName);
            _importDataTableName = SanitizeTableName(options.Value.TransactionsImportDataTableName);
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
                        description TEXT NOT NULL
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

        public Task<List<long>> AddTransactions(List<TransactionDto> transactions, List<long>? deleteTransactions = null)
        {
            var result = WithTransaction((conn, trns) =>
            {
                var added = AddTransactionsInternal(transactions, conn, trns);
                if (deleteTransactions?.Count > 0)
                    DeleteTransactionsInternal(deleteTransactions, conn, trns);
                return added;
            });
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
                        description)
                    VALUES
                    (@AmountCents,
                        @TimeStamp,
                        @Category,
                        @SourceAccount,
                        @DestinationAccount,
                        @Description);
                    SELECT last_insert_rowid();
                    ", conn, trns);

            var amountCentsParameter = insertCommand.Parameters.Add("@AmountCents", SqliteType.Integer);
            var timeStampParameter = insertCommand.Parameters.Add("@TimeStamp", SqliteType.Integer);
            var categoryParameter = insertCommand.Parameters.Add("@Category", SqliteType.Text);
            var sourceAccountParameter = insertCommand.Parameters.Add("@SourceAccount", SqliteType.Text);
            var destinationAccountParameter = insertCommand.Parameters.Add("@DestinationAccount", SqliteType.Text);
            var descriptionParameter = insertCommand.Parameters.Add("@Description", SqliteType.Text);

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
                timeStampParameter.Value = transaction.TimeStamp.UnixTimeSeconds;
                sourceAccountParameter.Value = transaction.SourceAccount;
                destinationAccountParameter.Value = transaction.DestinationAccount;
                descriptionParameter.Value = transaction.Description;

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


        public async Task<Optional<TransactionDto>> GetTransaction(long key)
        {
            var result = await GetTransactions(new List<TransactionFilter>
            {
                TransactionFilter.Id.IsOneOf([key])
            });

            return result.GetValue(key);
        }

        public async Task<bool> DeleteTransaction(long key) => (await DeleteTransactions([key])) > 0;

        public Task<int> DeleteTransactions(List<long> keys)
        {
            var result = WithTransaction((conn, trns) =>
            {
                return DeleteTransactionsInternal(keys, conn, trns);
            });

            return Task.FromResult(result);
        }
        public int DeleteTransactionsInternal(List<long> keys, SqliteConnection conn, SqliteTransaction trns)
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
            var (whereClauses, parameters, joinClauses) = GenerateWhereClauses(filters);
            foreach (var joinClause in joinClauses)
                baseQuery.AppendLine(joinClause);
            if (whereClauses.Count > 0)
                baseQuery.AppendLine($"WHERE {string.Join(" AND ", whereClauses)}");

            // Execute query
            var result = WithTransaction((conn, trns) =>
            {
                using var command = new SqliteCommand(baseQuery.ToString(), conn, trns);
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

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
            var (whereClauses, parameters, joinClauses) = GenerateWhereClauses(filters);
            foreach (var joinClause in joinClauses)
                baseQuery.AppendLine(joinClause);
            if (whereClauses.Count > 0)
                baseQuery.AppendLine($"WHERE {string.Join(" AND ", whereClauses)}");

            baseQuery.AppendLine($"GROUP BY {groupBy}");

            // Execute query
            using var command = new SqliteCommand(baseQuery.ToString(), conn);
            foreach (var parameter in parameters)
                command.Parameters.Add(parameter);

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
                    t.description
                FROM {_tableName} t
            ");
            var (whereClauses, parameters, joinClauses) = GenerateWhereClauses(filters);
            foreach (var joinClause in joinClauses)
                baseQuery.AppendLine(joinClause);
            if (whereClauses.Count > 0)
                baseQuery.AppendLine($"WHERE {string.Join(" AND ", whereClauses)}");

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

                var transactions = new Dictionary<long, TransactionDto>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var transaction = new TransactionDto
                    {
                        Amount = Convert.ToDecimal(reader["amountCents"]) / 100,
                        TimeStamp = AbsoluteDateTime.Create(Convert.ToInt64(reader["timeStamp"])),
                        Category = reader["category"].ToString() ?? "",
                        SourceAccount = reader["sourceAccount"].ToString() ?? "",
                        DestinationAccount = reader["destinationAccount"].ToString() ?? "",
                        Description = reader["description"].ToString() ?? "",
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
            var (whereClauses, parameters, joinClauses) = GenerateWhereClauses(filters);
            foreach (var joinClause in joinClauses)
                baseQuery.AppendLine(joinClause);
            if (whereClauses.Count > 0)
                baseQuery.AppendLine($"WHERE {string.Join(" AND ", whereClauses)}");

            // Execute query
            var result = WithConnection(conn =>
            {
                using var command = new SqliteCommand(baseQuery.ToString(), conn);
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);

                var transactions = new Dictionary<int, TransactionDto>();
                var result = command.ExecuteScalar();
                return Convert.ToInt64(result);
            });

            return Task.FromResult(result);
        }

        private (List<string> WhereClauses, List<SqliteParameter> Parameters, List<string> JoinClauses) GenerateWhereClauses(List<TransactionFilter> filters)
        {
            var whereClauses = new List<string>();
            var parameters = new List<SqliteParameter>();
            var joinClauses = new List<string>();
            var needsSupercategories = false;
            var needsSourceAccountName = false;
            var needsDestinationAccountName = false;
            var needsImportData = false;


            static NotSupportedException getCompatibilityException(TransactionFilter f, TransactionFilterOperation op)
            {
                return new NotSupportedException($"Transaction filter {f.GetType()} with operation {op.GetType()} is not supported.");
            }


            static void addClause<T>(
                List<string> whereClauses,
                List<SqliteParameter> parameters,
                int filterIndex,
                Union<T, List<T>> valueOrValues,
                Func<string, string> clauseFactory,
                string parameterPrefix = "where_clause_",
                Func<T, object>? prepareTerm = null) where T : notnull
            {
                if (valueOrValues.Is<List<T>>(out var values))
                {
                    var operationParameters = values
                        .Select((v, j) => ($"@{parameterPrefix}{filterIndex}_{j}", v));
                    var parametersString = string.Join(", ", operationParameters.Select(q => q.Item1));
                    whereClauses.Add(clauseFactory($"({parametersString})"));
                    if (prepareTerm != null)
                        parameters.AddRange(operationParameters.Select(q => new SqliteParameter(q.Item1, prepareTerm(q.Item2))));
                    else
                        parameters.AddRange(operationParameters.Select(q => new SqliteParameter(q.Item1, q.Item2)));
                }
                else
                {
                    var operationParameter = $"@{parameterPrefix}{filterIndex}";
                    whereClauses.Add(clauseFactory(operationParameter));
                    if (prepareTerm != null)
                        parameters.Add(new SqliteParameter(operationParameter, prepareTerm((T)valueOrValues)));
                    else
                        parameters.Add(new SqliteParameter(operationParameter, (T)valueOrValues));
                }
            }

            static void addClauseFromOperation<T>(
                List<string> whereClauses,
                List<SqliteParameter> parameters,
                int filterIndex,
                TransactionFilter<T> filter,
                Union<string, List<string>> columnSelector,
                Func<T, object>? prepareTerm = null) where T : notnull
            {
                static Func<string, string> generateClauseString(Union<string, List<string>> columnSelector, Func<string, string, string> template)
                {
                    if (columnSelector.Is<string>(out var singleColumnSelector))
                        return q => template(q, singleColumnSelector);
                    return q => string.Join(" OR ", ((List<string>)columnSelector).Select(r => template(q, r)));
                }

                switch (filter.Operation)
                {
                    case IsNoneTransactionFilterOperation<T>:
                        whereClauses.Add(generateClauseString(columnSelector, static (_, c) => $"{c} IS NULL")(""));
                        break;
                    case IsNotNoneTransactionFilterOperation<T>:
                        whereClauses.Add(generateClauseString(columnSelector, static (_, c) => $"{c} IS NOT NULL")(""));
                        break;
                    case IsNoneOrEqualToTransactionFilterOperation<T> isNoneOrOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isNoneOrOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} IS NULL OR {c} = {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    case IsOneOfTransactionFilterOperation<T> isOneOfOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isOneOfOp.Values,
                            generateClauseString(columnSelector, static (q, c) => $"{c} IN {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    case IsNotOneOfTransactionFilterOperation<T> isNotOneOfOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isNotOneOfOp.Values,
                            generateClauseString(columnSelector, static (q, c) => $"{c} NOT IN {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    case StartsWithTransactionFilterOperation startsWithOp:
                        addClause<string>(whereClauses, parameters, filterIndex, startsWithOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} LIKE {q}"),
                            prepareTerm: SqliteParameterCollectionExtensions.PrepareSqliteStartsWithTerm);
                        break;
                    case EndsWithTransactionFilterOperation endsWithOp:
                        addClause<string>(whereClauses, parameters, filterIndex, endsWithOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} LIKE {q}"),
                            prepareTerm: SqliteParameterCollectionExtensions.PrepareSqliteEndsWithTerm);
                        break;
                    case ContainsTransactionFilterOperation containsOp:
                        addClause<string>(whereClauses, parameters, filterIndex, containsOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} LIKE {q}"),
                            prepareTerm: SqliteParameterCollectionExtensions.PrepareSqliteContainsTerm);
                        break;
                    case IsGreaterThanTransactionFilterOperation<T> isGTOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isGTOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} > {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    case IsGreaterThanOrEqualToTransactionFilterOperation<T> isGTEOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isGTEOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} >= {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    case IsLessThanTransactionFilterOperation<T> isLTOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isLTOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} < {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    case IsLessThanOrEqualToTransactionFilterOperation<T> isLTEOp:
                        addClause<T>(whereClauses, parameters, filterIndex, isLTEOp.Value,
                            generateClauseString(columnSelector, static (q, c) => $"{c} <= {q}"),
                            prepareTerm: prepareTerm);
                        break;
                    default:
                        throw getCompatibilityException(filter, filter.Operation);


                }

            }

            var tagsFilters = new List<string>();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                switch (filter)
                {
                    case IdFilter idFilter:
                        addClauseFromOperation(whereClauses, parameters, i, idFilter, "t.id");
                        break;
                    case TagsFilter tagsFilter:
                        switch (tagsFilter.Operation)
                        {
                            case ContainsTransactionFilterOperation containsOp:
                                tagsFilters.Add(containsOp.Value); // handled seperately
                                break;
                            case IsNoneTransactionFilterOperation<string>:
                                whereClauses.Add($"NOT EXISTS (SELECT 1 FROM {_tagsTableName} where {_tagsTableName}.tid = t.id)");
                                break;
                            case IsNotNoneTransactionFilterOperation<string>:
                                whereClauses.Add($"EXISTS (SELECT 1 FROM {_tagsTableName} where {_tagsTableName}.tid = t.id)");
                                break;
                            default:
                                throw getCompatibilityException(tagsFilter, tagsFilter.Operation);
                        };
                        break;
                    case ImportSourceDataHashFilter importSourceDataHashFilter:
                        switch (importSourceDataHashFilter.Operation)
                        {
                            case IsNoneTransactionFilterOperation<long>:
                                whereClauses.Add($"NOT EXISTS (SELECT 1 FROM {_importDataTableName} where {_importDataTableName}.transactionId = t.id)");
                                break;
                            case IsNotNoneTransactionFilterOperation<long>:
                                whereClauses.Add($"EXISTS (SELECT 1 FROM {_importDataTableName} where {_importDataTableName}.transactionId = t.id)");
                                break;
                            default:
                                needsImportData = true;
                                addClauseFromOperation(whereClauses, parameters, i, importSourceDataHashFilter, $"{_importDataTableAlias}.sourceDataHash");
                                break;
                        };
                        break;
                    case DateFilter dateFilter:
                        addClauseFromOperation(whereClauses, parameters, i, dateFilter, "t.timeStamp", prepareTerm: static q => q.UnixTimeSeconds);
                        break;
                    case AmountFilter amountFilter:
                        addClauseFromOperation(whereClauses, parameters, i, amountFilter, "t.amountCents", prepareTerm: static q => (int)(q * 100));
                        break;
                    case CategoryFilter categoryFilter:
                        addClauseFromOperation(whereClauses, parameters, i, categoryFilter, "t.category");
                        break;
                    case DescriptionFilter descriptionFilter:
                        addClauseFromOperation(whereClauses, parameters, i, descriptionFilter, "t.description");
                        break;
                    case SourceAccountIdFilter sourceAccountIdFilter:
                        addClauseFromOperation(whereClauses, parameters, i, sourceAccountIdFilter, "t.sourceAccount");
                        break;
                    case DestinationAccountIdFilter destinationAccountIdFilter:
                        addClauseFromOperation(whereClauses, parameters, i, destinationAccountIdFilter, "t.destinationAccount");
                        break;
                    case EitherAccountIdFilter eitherAccountIdFilter:
                        addClauseFromOperation(whereClauses, parameters, i, eitherAccountIdFilter, new List<string> { "t.sourceAccount", "t.destinationAccount" });
                        break;
                    case SourceAccountNameFilter sourceAccountNameFilter:
                        needsSourceAccountName = true;
                        addClauseFromOperation(whereClauses, parameters, i, sourceAccountNameFilter, $"{_sourceAccountTableAlias}.name");
                        break;
                    case DestinationAccountNameFilter destinationAccountNameFilter:
                        needsDestinationAccountName = true;
                        addClauseFromOperation(whereClauses, parameters, i, destinationAccountNameFilter, $"{_destinationAccountTableAlias}.name");
                        break;
                    case EitherAccountNameFilter eitherAccountNameFilter:
                        needsSourceAccountName = true;
                        needsDestinationAccountName = true;
                        addClauseFromOperation(whereClauses, parameters, i, eitherAccountNameFilter, new List<string> { $"{_sourceAccountTableAlias}.name", $"{_destinationAccountTableAlias}.name" });
                        break;
                    case SupercategoryFilter supercategoryFilter:
                        needsSupercategories = true;
                        addClauseFromOperation(whereClauses, parameters, i, supercategoryFilter, $"{_supercategoriesTableAlias}.supercategory");
                        break;
                    default:
                        throw new NotSupportedException($"Transaction filter of type {filter.GetType()} is not supported.");
                }
            }


            if (tagsFilters.Count > 0)
            {
                var tagsParameters = tagsFilters.Distinct()
                    .Select((t, i) => ($"@Tag{i}", t)).ToList();
                var tagsSelectQueries = tagsParameters
                    .Select(t => $"SELECT tid FROM {_tagsTableName} WHERE name = {t.Item1}");
                var tagsQuery = string.Join(" INTERSECT ", tagsSelectQueries);
                whereClauses.Add($"t.id IN ({tagsQuery})");
                parameters.AddRange(tagsParameters.Select(t => new SqliteParameter(t.Item1, t.t)));
            }

            if (needsSupercategories)
                joinClauses.Add($"LEFT JOIN {_supercategoriesTableName} {_supercategoriesTableAlias} ON t.category = {_supercategoriesTableAlias}.category");
            if (needsSourceAccountName)
                joinClauses.Add($"LEFT JOIN {_accountsTableName} {_sourceAccountTableAlias} ON t.sourceAccount = {_sourceAccountTableAlias}.id");
            if (needsDestinationAccountName)
                joinClauses.Add($"LEFT JOIN {_accountsTableName} {_destinationAccountTableAlias} ON t.destinationAccount = {_destinationAccountTableAlias}.id");
            if (needsImportData)
                joinClauses.Add($"LEFT JOIN {_importDataTableName} {_importDataTableAlias} ON t.id = {_importDataTableAlias}.transactionId");


            return (whereClauses, parameters, joinClauses);
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
