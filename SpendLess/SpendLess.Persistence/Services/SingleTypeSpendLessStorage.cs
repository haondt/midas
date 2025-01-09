using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using SpendLess.Persistence.Exceptions;

namespace SpendLess.Persistence.Services
{
    public class SingleTypeSpendLessStorage<T>(IStorage storage) : ISingleTypeSpendLessStorage<T> where T : notnull
    {
        private StorageKey<T> TypeForeignKey { get; } = StorageKey<SingleTypeSpendLessStorage<T>>.Empty.Extend<T>();

        public async Task<T> Get(StorageKey<T> primaryKey)
        {
            var result = await storage.Get(primaryKey);
            if (!result.IsSuccessful)
                throw new StorageException($"Couldn't find {typeof(T).Name} with id {primaryKey}");
            return result.Value;
        }

        public async Task<Optional<T>> TryGet(StorageKey<T> primaryKey)
        {
            var result = await storage.Get(primaryKey);
            if (!result.IsSuccessful)
                return new();
            return result.Value;
        }

        public async Task<List<Optional<T>>> TryGetMany(List<StorageKey<T>> primaryKeys)
        {
            var result = await storage.GetMany<T>(primaryKeys);
            return result.Select(x => x.IsSuccessful ? new Optional<T>(x.Value) : new()).ToList();
        }

        public async Task<Dictionary<StorageKey<T>, T>> GetAll()
        {
            var results = await storage.GetManyByForeignKey(TypeForeignKey);
            return results.DistinctBy(k => k.Key)
                .ToDictionary(keySelector => keySelector.Key, keySelector => keySelector.Value);
        }

        public async Task Set(StorageKey<T> primarkey, T value)
        {
            await storage.Set(primarkey, value, [TypeForeignKey]);
        }

        public Task Delete(StorageKey<T> primaryKey)
        {
            return storage.Delete(primaryKey);
        }

        public Task<bool> ContainsKey(StorageKey<T> primaryKey)
        {
            return storage.ContainsKey(primaryKey);
        }

        public async Task<bool> ContainsForeignKey(StorageKey<T> foreignKey)
        {
            return (await storage.GetManyByForeignKey(foreignKey)).Count > 0;
        }

        public Task Set(StorageKey<T> primarkey, T value, List<StorageKey<T>> foreignKeys)
        {
            return storage.Set(primarkey, value, foreignKeys.Append(TypeForeignKey).ToList());
        }
    }
}
