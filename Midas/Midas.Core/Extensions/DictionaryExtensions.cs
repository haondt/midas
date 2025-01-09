using Haondt.Core.Models;

namespace Midas.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static Optional<TValue> GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
                return new(value);
            return new();
        }
    }
}
