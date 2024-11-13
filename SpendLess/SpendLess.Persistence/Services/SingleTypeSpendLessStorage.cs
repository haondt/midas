﻿using Haondt.Core.Models;
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
    }
}
