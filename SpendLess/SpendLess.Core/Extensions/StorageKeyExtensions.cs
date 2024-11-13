using Haondt.Identity.StorageKey;
using SpendLess.Core.Extensions;

namespace SpendLess.Core.Extensions
{
    public static class StorageKeyExtensions
    {
        public static StorageKeyPart Single(this StorageKey storageKey) => storageKey.Parts.Single();
        public static string SingleValue(this StorageKey storageKey) => storageKey.Parts.Single().Value;
        public static T SingleValue<T>(this StorageKey storageKey) where T : notnull => storageKey.Parts.Single().Value<T>();
    }
}
