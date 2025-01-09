using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;

namespace Midas.Persistence.Extensions
{
    public static class StorageExtensions
    {
        public static async Task<T> GetDefault<T>(this IStorage storage, StorageKey<T> key) where T : notnull, new()
        {
            var result = await storage.Get<T>(key);
            if (result.IsSuccessful)
                return result.Value;
            return new();
        }

        public static async Task<T> GetDefault<T>(this IStorage storage, StorageKey<T> key, T defaultValue) where T : notnull
        {
            var result = await storage.Get<T>(key);
            if (result.IsSuccessful)
                return result.Value;
            return defaultValue;
        }

        public static async Task<T> GetDefault<T>(this IStorage storage, StorageKey<T> key, Func<T> defaultValueFactory) where T : notnull
        {
            var result = await storage.Get<T>(key);
            if (result.IsSuccessful)
                return result.Value;
            return defaultValueFactory();
        }

        public static async Task<T> GetDefault<T>(this IStorage storage, StorageKey<T> key, Func<Task<T>> defaultValueFactory) where T : notnull
        {
            var result = await storage.Get<T>(key);
            if (result.IsSuccessful)
                return result.Value;
            return await defaultValueFactory();
        }

    }
}
