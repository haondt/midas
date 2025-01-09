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
    }
}