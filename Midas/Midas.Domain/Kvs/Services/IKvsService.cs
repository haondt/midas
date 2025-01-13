using Haondt.Core.Models;
using Midas.Domain.Kvs.Models;

namespace Midas.Domain.Kvs.Services
{
    public interface IKvsService
    {
        Task<List<string>> AddAlias(string key, string alias);
        Task<KvsMapping> GetMapping(string term);
        Task<Optional<string>> TryGetKeyFromKeyOrAlias(string term);
        Task<Optional<string>> GetValueFromKeyOrAlias(string key);
        Task<Optional<(string Key, string Value)>> GetKeyAndValueFromKeyOrAlias(string key);
        Task<List<string>> DeleteAlias(string alias);
        Task<List<string>> Search(string term);
        Task UpsertValue(string key, string value);
        Task ImportKvsMappings(ExternalKvsMappingsDto mappings, bool overwriteExisting);
        Task<ExternalKvsMappingsDto> ExportMappings();
        Task DeleteMapping(string key);
        Task MoveMapping(string oldKey, string newKey);
        Task<int> DeleteAllMappings();
    }
}
