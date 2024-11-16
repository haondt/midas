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
    }
}
