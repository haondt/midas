using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Kvs.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Storages;

namespace SpendLess.Kvs.Services
{
    public class KvsService(IStorage storage, IKvsStorage kvsStorage,
        ILogger<KvsService> logger) : IKvsService
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
            logger.LogInformation("Starting mapping import");
            try
            {

                foreach (var (key, value) in mappings.Mappings)
                {
                    var storageKey = key.SeedStorageKey<KvsMappingDto>();

                    logger.LogInformation($"importing key {key}...");

                    if (!string.IsNullOrEmpty(value.Value))
                        if (overwriteExisting || !await storage.ContainsKey(storageKey))
                            await UpsertValue(key, value.Value);

                    if (value.Aliases != null)
                        foreach (var alias in value.Aliases)
                            await AddAlias(key, alias);

                }
                logger.LogInformation("Completed mapping import");
            }
            catch (Exception)
            {
                logger.LogInformation("Ran into an error during mapping import");
                throw;
            }
        }

        public async Task<ExternalKvsMappingsDto> ExportMappings()
        {
            var result = new ExternalKvsMappingsDto();

            foreach (var key in await kvsStorage.GetAllKeys())
            {
                var externalMapping = new ExternalKvsMappingDto();

                var storageKey = key.SeedStorageKey<KvsMappingDto>();
                var mapping = await storage.Get(storageKey);
                if (mapping.IsSuccessful)
                    externalMapping.Value = mapping.Value.Value;

                var aliasForeignKey = storageKey.Extend<KvsAliasDto>();
                var aliases = await storage.GetManyByForeignKey(aliasForeignKey);
                foreach (var (alias, aliasValue) in aliases)
                {
                    // shouldn't happen but just in case
                    if (aliasValue.Key != storageKey)
                        continue;

                    externalMapping.Aliases ??= [];
                    externalMapping.Aliases.Add(alias.SingleValue());
                }

                if (externalMapping.Aliases != null || !string.IsNullOrEmpty(externalMapping.Value))
                    result.Mappings[key] = externalMapping;
            }

            return result;
        }

        public async Task<Optional<(string Key, KvsMappingDto Value)>> GetKeyAndValueFromKeyOrAlias(string term)
        {
            var storageKey = term.SeedStorageKey<KvsMappingDto>();
            var aliasKey = term.SeedStorageKey<KvsAliasDto>();
            var aliasResult = await storage.Get(aliasKey);
            if (aliasResult.IsSuccessful)
            {
                storageKey = aliasResult.Value.Key;
                var keyString = storageKey.LastValue();
                var keyResult = await storage.Get(storageKey);
                if (keyResult.IsSuccessful)
                    return (keyString, keyResult.Value);
                // alias is pointing to a key that doesn't exist (yet)
                return new((keyString, new()));
            }

            var result = await storage.Get(storageKey);
            if (result.IsSuccessful)
                return (term, result.Value);


            return new();
        }
    }
}
