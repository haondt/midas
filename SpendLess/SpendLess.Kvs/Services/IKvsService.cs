using Haondt.Core.Models;
using SpendLess.Domain.Models;
using SpendLess.Kvs.Models;

namespace SpendLess.Kvs.Services
{
    public interface IKvsService
    {
        Task<List<string>> AddAlias(string key, string alias);
        Task<Optional<KvsMappingDto>> GetValueFromKeyOrAlias(string key);
        Task<ExpandedKvsMappingDto> GetExpandedMapping(string term);
        Task<Optional<string>> GetKeyFromKeyOrAlias(string term);
        Task<List<string>> RemoveAlias(string key, string alias);
        Task<List<string>> Search(string term);
        Task UpsertValue(string key, string value);
    }
}
