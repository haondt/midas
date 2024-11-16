using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using SpendLess.Kvs.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Models;
using SpendLess.Persistence.Storages;

namespace SpendLess.Kvs.Services
{
    public class KvsService(IStorage storage, IKvsStorage kvsStorage) : IKvsService
    {
        public async Task<ExpandedKvsMappingDto> GetExpandedMapping(string term)
        {
            var key = term.SeedStorageKey<KvsMappingDto>();
            var foreignKeys = await storage.GetForeignKeys(key);
            var aliases = foreignKeys.Select(fk => fk.SingleValue()).ToList();

            var result = await storage.GetDefault(key);
            return new ExpandedKvsMappingDto
            {
                Key = term,
                Aliases = aliases,
                Value = result.Value
            };
        }

        public async Task<List<string>> Search(string term)
        {
            var matches = await kvsStorage.SearchKey(term);
            return matches;
        }

        public async Task UpsertValue(string key, string value)
        {
            await kvsStorage.AddKey(key);
            var storageKey = key.SeedStorageKey<KvsMappingDto>();
            await storage.Set(storageKey, new KvsMappingDto { Value = value });
        }

        public async Task<List<string>> AddAlias(string key, string alias)
        {
            await kvsStorage.AddKey(key);
            var storageKey = key.SeedStorageKey<KvsMappingDto>();
            var operations = new List<StorageOperation<KvsMappingDto>>();
            if (!await storage.ContainsKey(storageKey))
                operations.Add(new SetOperation<KvsMappingDto>
                {
                    Target = storageKey,
                    Value = new()
                });

            operations.Add(new AddForeignKeyOperation<KvsMappingDto>
            {
                ForeignKey = alias.SeedStorageKey<KvsMappingDto>(),
                Target = storageKey
            });
            await storage.PerformTransactionalBatch(operations);

            var aliases = await storage.GetForeignKeys(storageKey);
            return aliases.Select(a => a.SingleValue()).ToList();
        }

        public async Task<List<string>> RemoveAlias(string key, string alias)
        {
            var storageKey = key.SeedStorageKey<KvsMappingDto>();
            await storage.PerformTransactionalBatch(new List<StorageOperation<KvsMappingDto>>
            {
                new DeleteForeignKeyOperation<KvsMappingDto>
                {
                    Target = alias.SeedStorageKey<KvsMappingDto>(),
                }
            });

            var aliases = await storage.GetForeignKeys(storageKey);
            return aliases.Select(a => a.SingleValue()).ToList();
        }
    }
}
