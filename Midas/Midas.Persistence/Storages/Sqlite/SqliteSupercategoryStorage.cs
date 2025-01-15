using Haondt.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Midas.Persistence.Extensions;
using Midas.Persistence.Services;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Persistence.Storages.Sqlite
{
    public class SqliteSupercategoryStorage : AbstractSqliteStorage, ISupercategoryStorage
    {
        private readonly string _tableName;

        public SqliteSupercategoryStorage(IOptions<MidasPersistenceSettings> options) : base(options)
        {
            _tableName = SanitizeTableName(options.Value.SupercategoryTableName);
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
                            category TEXT PRIMARY KEY NOT NULL,
                            supercategory TEXT NOT NULL
                     );", connection, transaction);
                createPrimaryTableCommand.ExecuteNonQuery();
            });
        }

        public Task<Optional<string>> GetSupercategory(string category)
        {
            var result = WithConnection(connection =>
            {
                var query = $"SELECT supercategory FROM {_tableName} where category = @category;";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@category", category);

                return command.ExecuteScalar() as string;
            });

            return Task.FromResult(result != null ? new Optional<string>(result) : new());
        }

        public Task<List<string>> SearchSupercategories(string partialName, int limit)
        {
            var result = WithConnection(connection =>
            {
                List<string> matches = [];
                using var searchCommand = new SqliteCommand(
                    $@"
                        SELECT DISTINCT supercategory
                        FROM {_tableName}
                        WHERE supercategory LIKE @partialName
                        LIMIT {limit};", connection);

                searchCommand.Parameters.AddLikeTermWithValue("@partialName", partialName);

                using var reader = searchCommand.ExecuteReader();
                while (reader.Read())
                    matches.Add(reader.GetString(0));
                return matches;
            });

            return Task.FromResult(result);
        }

        public Task<List<string>> GetCategories(string supercategory)
        {
            return Task.FromResult(WithConnection(connection =>
            {
                var query = $"SELECT category FROM {_tableName} WHERE supercategory = @supercategory;";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@supercategory", supercategory);

                using var reader = command.ExecuteReader();
                var categories = new List<string>();
                while (reader.Read())
                    categories.Add(reader.GetString(0));
                return categories;
            }));
        }

        public Task<List<string>> GetAllCategories()
        {
            return Task.FromResult(WithConnection(connection =>
            {
                var query = $"SELECT category FROM {_tableName};";
                using var command = new SqliteCommand(query, connection);

                using var reader = command.ExecuteReader();
                var categories = new List<string>();
                while (reader.Read())
                    categories.Add(reader.GetString(0));
                return categories;
            }));
        }

        public Task<Dictionary<string, List<string>>> GetAllSupercategories()
        {
            return Task.FromResult(WithConnection(connection =>
            {
                var query = $"SELECT supercategory, category FROM {_tableName};";
                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                var supercategories = new Dictionary<string, List<string>>();
                while (reader.Read())
                {
                    var supercategory = reader.GetString(0);
                    var category = reader.GetString(1);

                    if (!supercategories.TryGetValue(supercategory, out var categories))
                        categories = supercategories[supercategory] = [];
                    categories.Add(category);
                }
                return supercategories;
            }));

        }

        public Task SetSupercategory(string category, string supercategory) => SetManySupercategories([(category, supercategory)]);

        public Task SetManySupercategories(IEnumerable<(string category, string supercategory)> items)
        {
            WithTransaction((connection, transaction) =>
            {
                using var command = new SqliteCommand($"INSERT OR REPLACE INTO {_tableName} (category, supercategory) VALUES (@category, @supercategory);", connection, transaction);
                var categoryParameter = command.Parameters.Add("@category", SqliteType.Text);
                var supercategoryParameter = command.Parameters.Add("@supercategory", SqliteType.Text);

                foreach (var (category, supercategory) in items)
                {
                    categoryParameter.Value = category;
                    supercategoryParameter.Value = supercategory;
                    command.ExecuteNonQuery();
                }
            });

            return Task.CompletedTask;
        }

        public Task AddManySupercategories(IEnumerable<(string category, string supercategory)> items)
        {
            WithTransaction((connection, transaction) =>
            {
                using var command = new SqliteCommand($"INSERT INTO {_tableName} (category, supercategory) VALUES (@category, @supercategory);", connection, transaction);
                var categoryParameter = command.Parameters.Add("@category", SqliteType.Text);
                var supercategoryParameter = command.Parameters.Add("@supercategory", SqliteType.Text);

                foreach (var (category, supercategory) in items)
                {
                    categoryParameter.Value = category;
                    supercategoryParameter.Value = supercategory;
                    command.ExecuteNonQuery();
                }
            });

            return Task.CompletedTask;
        }

        public Task UnsetSupercategory(string category) => UnsetManySupercategories([category]);

        public Task UnsetManySupercategories(List<string> categories)
        {
            WithTransaction((connection, transaction) =>
            {
                using var command = new SqliteCommand($"DELETE FROM {_tableName} WHERE category = @category;", connection, transaction);
                var categoryParameter = command.Parameters.Add("@category", SqliteType.Text);

                foreach (var category in categories)
                {
                    categoryParameter.Value = category;
                    command.ExecuteNonQuery();
                }
            });

            return Task.CompletedTask;
        }

        public Task DeleteSupercategory(string supercategory)
        {
            WithConnection((connection) =>
            {
                using var command = new SqliteCommand($"DELETE FROM {_tableName} WHERE supercategory = @supercategory;", connection);
                command.Parameters.AddWithValue("@supercategory", supercategory);
                command.ExecuteNonQuery();
            });

            return Task.CompletedTask;
        }

        public Task DeleteAllSupercategories()
        {
            WithConnection((connection) =>
            {
                using var command = new SqliteCommand($"DELETE FROM {_tableName};", connection);
                command.ExecuteNonQuery();
            });

            return Task.CompletedTask;
        }
    }
}
