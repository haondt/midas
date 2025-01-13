
using Haondt.Core.Models;

namespace Midas.Persistence.Storages.Abstractions
{
    public interface IKvsStorage
    {
        Task<List<string>> AddAlias(string key, string alias);
        Task AddMappings(List<(string Key, string Value, List<string> Aliases)> mappings, bool overwriteExisting);
        Task<List<string>> DeleteAlias(string alias);
        Task DeleteKey(string key);
        Task<List<(string Key, string Value, List<string> Aliases)>> GetAllMappings();
        Task<(string Value, List<string> Aliases)> GetMapping(string key);
        Task<List<string>> SearchKey(string partialKeyOrAlias);
        Task<Optional<string>> TryGetKeyFromKeyOrAlias(string keyOrAlias);
        Task<Optional<string>> TryGetValueFromKeyOrAlias(string keyOrAlias);
        Task UpsertKeyAndValue(string key, string value);
        Task<Optional<(string Key, string Value)>> TryGetKeyAndValueFromKeyOrAlias(string keyOrAlias);
        Task MoveMapping(string oldKey, string newKey);
        Task<int> DeleteAll();
    }
}
