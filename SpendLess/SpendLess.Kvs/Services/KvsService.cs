using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Kvs.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Storages;

namespace SpendLess.Kvs.Services
{
    public class KvsService(IStorage storage, IKvsStorage kvsStorage) : IKvsService
    {
        public async Task<ExpandedKvsMappingDto> GetExpandedMapping(string term)
        {
            var storageKey = term.SeedStorageKey<KvsMappingDto>();
            var aliasForeignKey = storageKey.Extend<KvsAliasDto>();
            var aliasDtos = await storage.GetManyByForeignKey(aliasForeignKey);
            var aliases = aliasDtos.Select(dto => dto.Key.SingleValue()).ToList();

            var result = await storage.GetDefault(storageKey);

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

        public async Task<Optional<string>> GetKeyFromKeyOrAlias(string term)
        {
            var storageKey = term.SeedStorageKey<KvsMappingDto>();
            var keyResult = await storage.Get(storageKey);
            if (keyResult.IsSuccessful)
                return term;

            var aliasKey = term.SeedStorageKey<KvsAliasDto>();
            var aliasResult = await storage.Get(aliasKey);
            if (aliasResult.IsSuccessful)
                return aliasResult.Value.Key.SingleValue();

            return new();
        }

        public async Task<Optional<KvsMappingDto>> GetValueFromKeyOrAlias(string term)
        {
            var storageKey = term.SeedStorageKey<KvsMappingDto>();
            var keyResult = await storage.Get(storageKey);
            if (keyResult.IsSuccessful)
                return keyResult.Value;

            var aliasKey = term.SeedStorageKey<KvsAliasDto>();
            var aliasResult = await storage.Get(aliasKey);
            if (aliasResult.IsSuccessful)
            {
                keyResult = await storage.Get(aliasResult.Value.Key);
                if (keyResult.IsSuccessful)
                    return keyResult.Value;
                else
                    // alias is pointing to a key that doesn't exist (yet)
                    return new(new());
            }

            return new();
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
            var aliasKey = alias.SeedStorageKey<KvsAliasDto>();
            var aliasForeignKey = storageKey.Extend<KvsAliasDto>();

            var operations = new List<StorageOperation<KvsAliasDto>>()
            {
                new SetOperation<KvsAliasDto>
                {
                    Target = aliasKey,
                    Value = new() { Key = storageKey }
                },
                new AddForeignKeyOperation<KvsAliasDto>
                {
                    Target = aliasKey,
                    ForeignKey = aliasForeignKey
                }
            };
            await storage.PerformTransactionalBatch(operations);

            var aliases = await storage.GetManyByForeignKey(aliasForeignKey);
            return aliases.Select(a => a.Key.SingleValue()).ToList();
        }

        public async Task<List<string>> RemoveAlias(string key, string alias)
        {
            var storageKey = key.SeedStorageKey<KvsMappingDto>();
            var aliasKey = alias.SeedStorageKey<KvsAliasDto>();
            var aliasForeignKey = storageKey.Extend<KvsAliasDto>();
            await storage.Delete(aliasKey);
            var aliases = await storage.GetManyByForeignKey(aliasForeignKey);
            return aliases.Select(a => a.Key.SingleValue()).ToList();
        }

        public async Task ImportKvsMappings(ExternalKvsMappingsDto mappings, bool overwriteExisting)
        {
            foreach (var (key, value) in mappings.Mappings)
            {
                if (overwriteExisting)
                {
                    await UpsertValue(key, value);
                    continue;
                }

                var storageKey = key.SeedStorageKey<KvsMappingDto>();
                if (await storage.ContainsKey(storageKey))
                    continue;

                await kvsStorage.AddKey(key);
                await storage.Set(storageKey, new KvsMappingDto { Value = value });
            }

            foreach (var (alias, key) in mappings.Aliases)
                await AddAlias(key, alias);
        }

        public async Task<ExternalKvsMappingsDto> ExportMappings()
        {
            var result = new ExternalKvsMappingsDto();

            foreach (var key in await kvsStorage.GetAllKeys())
            {
                var storageKey = key.SeedStorageKey<KvsMappingDto>();
                var mapping = await storage.Get(storageKey);
                if (!mapping.IsSuccessful)
                    continue;

                result.Mappings[key] = mapping.Value.Value;

                var aliasForeignKey = storageKey.Extend<KvsAliasDto>();
                var aliases = await storage.GetManyByForeignKey(aliasForeignKey);
                foreach (var (alias, aliasValue) in aliases)
                {
                    // shouldn't happen but just in case
                    if (aliasValue.Key != storageKey)
                        continue;

                    result.Aliases[alias.SingleValue()] = key;
                }
            }

            return result;

        }
    }
}
