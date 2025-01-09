using Haondt.Identity.StorageKey;

namespace Midas.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static StorageKey<T> SeedStorageKey<T>(this object obj) where T : notnull
        {
            return StorageKey<T>.Create(obj.ToString()
                ?? throw new InvalidOperationException($"could not convert object {obj} of type {obj.GetType()} to string"));
        }
    }
}
