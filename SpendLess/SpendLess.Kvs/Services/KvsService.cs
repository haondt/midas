using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Kvs.Models;
using SpendLess.Persistence.Extensions;

namespace SpendLess.Kvs.Services
{
    public class KvsService(IStorage storage) : IKvsService
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

        public async Task<Optional<string>> Search(string term)
        {
            var key = term.SeedStorageKey<KvsMappingDto>();
            if (await storage.ContainsKey(key))
                return term;

            var keys = await storage.GetManyByForeignKey(key);
            if (keys.Count > 0)
                return keys.First().Key.SingleValue();

            return new();
        }

        public Task UpsertValue(string key, string value)
        {
            var storageKey = key.SeedStorageKey<KvsMappingDto>();
            return storage.Set(storageKey, new KvsMappingDto { Value = value });
        }

        public async Task<List<string>> AddAlias(string key, string alias)
        {
            var storageKey = key.SeedStorageKey<KvsMappingDto>();
            await storage.PerformTransactionalBatch(new List<StorageOperation<KvsMappingDto>>
            {
                new AddForeignKeyOperation<KvsMappingDto>
                {
                    ForeignKey = alias.SeedStorageKey<KvsMappingDto>(),
                    Target = storageKey
                }
            });

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
