
using Microsoft.Data.Sqlite;

namespace SpendLess.Persistence.Extensions
{
    public static class SqliteParameterCollectionExtensions
    {
        public static SqliteParameter AddLikeTermWithValue(this SqliteParameterCollection collection, string parameterName, string term)
        {
            return collection.AddWithValue(parameterName, PrepareSqliteLikeTerm(term));
        }

        public static string PrepareSqliteLikeTerm(string term)
        {
            term = term.Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("\\", "[\\]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");

            return $"%{term}%";
        }
    }
}
