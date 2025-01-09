using Haondt.Core.Models;
using Midas.Domain.Kvs.Models;

namespace Midas.Domain.Kvs.Services
{
    public interface IKvsService
    {
        Task<List<string>> AddAlias(string key, string alias);
        Task<ExpandedKvsMappingDto> GetExpandedMapping(string term);
        Task<Optional<string>> GetKeyFromKeyOrAlias(string term);
        Task<Optional<KvsMappingDto>> GetValueFromKeyOrAlias(string key);
        Task<Optional<(string Key, KvsMappingDto Value)>> GetKeyAndValueFromKeyOrAlias(string key);
        Task<List<string>> RemoveAlias(string key, string alias);
        Task<List<string>> Search(string term);
        Task UpsertValue(string key, string value);
        Task ImportKvsMappings(ExternalKvsMappingsDto mappings, bool overwriteExisting);
        Task<ExternalKvsMappingsDto> ExportMappings();
    }
}
