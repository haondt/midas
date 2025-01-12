using Haondt.Core.Models;
using Midas.Domain.Kvs.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Kvs.Services
{
    public class KvsService(IKvsStorage kvsStorage) : IKvsService
    {
        public async Task<KvsMapping> GetMapping(string key)
        {
            var mapping = await kvsStorage.GetMapping(key);
            return new KvsMapping
            {
                Value = mapping.Value,
                Key = key,
                Aliases = mapping.Aliases
            };
        }

        public async Task<List<string>> Search(string term)
        {
            var matches = await kvsStorage.SearchKey(term);
            return matches;
        }

        public Task<Optional<string>> TryGetKeyFromKeyOrAlias(string keyOrAlias)
        {
            return kvsStorage.TryGetKeyFromKeyOrAlias(keyOrAlias);
        }

        public Task<Optional<string>> GetValueFromKeyOrAlias(string keyOrAlias)
        {
            return kvsStorage.TryGetValueFromKeyOrAlias(keyOrAlias);
        }

        public async Task UpsertValue(string key, string value)
        {
            await kvsStorage.UpsertKeyAndValue(key, value);
        }

        public Task<List<string>> AddAlias(string key, string alias)
        {
            return kvsStorage.AddAlias(key, alias);
        }

        public Task<List<string>> DeleteAlias(string alias)
        {
            return kvsStorage.DeleteAlias(alias);
        }

        public Task ImportKvsMappings(ExternalKvsMappingsDto mappings, bool overwriteExisting)
        {
            return kvsStorage.AddMappings(mappings.Mappings
                .Select(m => (m.Key, m.Value.Value ?? string.Empty, m.Value.Aliases ?? []))
                .ToList(), overwriteExisting);
        }

        public async Task<ExternalKvsMappingsDto> ExportMappings()
        {
            var result = new ExternalKvsMappingsDto();
            result.Mappings = (await kvsStorage.GetAllMappings())
                .ToDictionary(q => q.Key, q => new ExternalKvsMappingDto
                {
                    Aliases = q.Aliases,
                    Value = q.Value
                });

            return result;
        }

        public Task<Optional<(string Key, string Value)>> GetKeyAndValueFromKeyOrAlias(string keyOrAlias)
        {
            return kvsStorage.TryGetKeyAndValueFromKeyOrAlias(keyOrAlias);
        }
    }
}
