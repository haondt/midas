using Haondt.Core.Models;
using SpendLess.Kvs.Models;

namespace SpendLess.Kvs.Services
{
    public interface IKvsService
    {
        Task<ExpandedKvsMappingDto> GetExpandedMapping(string term);
        Task<Optional<string>> Search(string term);
        Task UpsertValue(string key, string value);
    }
}
