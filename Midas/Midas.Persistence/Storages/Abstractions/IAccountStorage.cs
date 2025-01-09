using Haondt.Core.Models;
using Midas.Persistence.Models;

namespace Midas.Persistence.Storages.Abstractions
{
    public interface IAccountStorage
    {
        Task<AccountDto> Get(string id);
        Task<Optional<AccountDto>> TryGet(string id);
        Task Set(string accountId, AccountDto account);
        Task SetMany(List<(string AccountId, AccountDto Account)> values);
        Task<Dictionary<string, AccountDto>> GetAll(long? limit = null, long? offset = null);
        Task<List<(string Name, string Id)>> SearchAccountsByName(string partialName, int limit);
        Task<bool> HasAccountWithName(string name);
        Task<Dictionary<string, AccountDto>> GetAllMine();
        Task<bool> Delete(string id);
        Task<int> Delete(List<string> ids);
        Task<int> DeleteAll();
        Task<long> GetCount();
        Task AddMany(List<(string AccountId, AccountDto Account)> values);
        Task Add(string accountId, AccountDto account);
        Task<List<string>> GetAccountIdsByName(string name, long? limit = null);
        Task<Dictionary<string, AccountDto>> GetMany(List<string> ids, long? limit = null, long? offset = null);
    }
}
