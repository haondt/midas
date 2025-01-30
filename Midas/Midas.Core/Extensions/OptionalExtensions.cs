using Haondt.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace Midas.Core.Extensions
{

    public static class OptionalExtensions
    {

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

        public static Optional<T2> Cast<T1, T2>(this Optional<T1> optional) where T1 : notnull where T2 : notnull
        {
            if (!optional.HasValue)
                return new();
            if (optional.Value is not T2 casted)
                throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T2)}");
            return casted;
        }
        public static Optional<T2> TryCast<T1, T2>(this Optional<T1> optional) where T1 : notnull where T2 : notnull
        {
            if (!optional.HasValue)
                return new();
            if (optional.Value is not T2 casted)
                return new();
            return casted;
        }
    }
}