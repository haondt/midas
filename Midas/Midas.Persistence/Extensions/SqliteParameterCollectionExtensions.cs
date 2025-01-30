
using Microsoft.Data.Sqlite;

namespace Midas.Persistence.Extensions
{
    public static class SqliteParameterCollectionExtensions
    {
        public static SqliteParameter AddLikeTermWithValue(this SqliteParameterCollection collection, string parameterName, string term)
        {
            return collection.AddWithValue(parameterName, PrepareSqliteContainsTerm(term));
        }

        public static string PrepareSqliteContainsTerm(string term)
        {
            return $"%{EscapeLikeTerm(term)}%";
        }
        public static string PrepareSqliteStartsWithTerm(string term)
        {
            return $"{EscapeLikeTerm(term)}%";
        }
        public static string PrepareSqliteEndsWithTerm(string term)
        {
            return $"%{EscapeLikeTerm(term)}";
        }
        private static string EscapeLikeTerm(string term)
        {
            return term.Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("\\", "[\\]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
        }
    }
}
