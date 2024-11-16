using SpendLess.Kvs.Models;

namespace SpendLess.Kvs.Services
{
    public interface IKvsService
    {
        Task<List<string>> AddAlias(string key, string alias);
        Task<ExpandedKvsMappingDto> GetExpandedMapping(string term);
        Task<List<string>> RemoveAlias(string key, string alias);
        Task<List<string>> Search(string term);
        Task UpsertValue(string key, string value);
    }
}
