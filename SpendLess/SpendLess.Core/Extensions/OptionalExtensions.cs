using Haondt.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace SpendLess.Core.Extensions
{

    public static class OptionalExtensions
    {

        public static T Or<T>(this Optional<T> optional, T defaultValue) where T : notnull
        {
            if (optional.HasValue)
                return optional.Value;
            return defaultValue;
        }
        public static T Or<T>(this Optional<T> optional, Func<T> defaultValueFactory) where T : notnull
        {
            if (optional.HasValue)
                return optional.Value;
            return defaultValueFactory();
        }
        public static bool Test<T>(this Optional<T> optional, [MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (optional.HasValue)
            {
                value = optional.Value;
                return true;
            }
            value = default;
            return false;
        }
    }
}