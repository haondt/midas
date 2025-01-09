using Haondt.Core.Models;
using SpendLess.Domain.Accounts.Models;
using SpendLess.Persistence.Models;

namespace SpendLess.Domain.Accounts.Services
{
    public interface IAccountsService
    {
        Task<AccountDetails> GetDetails(string id);
        Task DeleteAccount(string id);
        Task<AccountDto> GetAccount(string id);
        Task CreateAccount(string id, AccountDto accountDto);
        Task UpsertAccount(string id, AccountDto accountDto);
        Task<Dictionary<string, AccountDto>> GetAccountsIncludedInNetWorth();
        Task<Optional<AccountDto>> TryGetAccount(string id);
        Task<bool> HasAccountWithName(string accountName);
        Task<Dictionary<string, AccountDetails>> GetPagedDetails(int pageSize, int page);
        Task<long> GetNumberOfAccounts();
        Task<int> DeleteAllAccounts();
        Task<List<(string Name, string Id)>> SearchAccountsByName(string partialName);
        Task<List<string>> GetAccountIdsByName(string name, long? limit = null);
        Task<Dictionary<string, AccountDto>> GetMany(List<string> ids);
        Task<Optional<string>> GetAccountIdByName(string name);
        Task UpsertAccounts(List<(string Id, AccountDto Account)> accounts);
        Task CreateAccounts(List<(string Id, AccountDto Account)> accounts);
    }
}
