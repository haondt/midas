

using Haondt.Core.Models;
using SpendLess.Accounts.Models;
using SpendLess.Domain.Models;

namespace SpendLess.Accounts.Services
{
    public interface IAccountsService
    {
        Task<AccountDetails> GetDetails(string id);
        Task DeleteAccount(string id);
        Task<AccountDto> GetAccount(string id);
        Task IncludeInNetWorth(string id);
        Task<List<bool>> IsIncludedInNetWorth(List<string> ids);
        Task<bool> IsIncludedInNetWorth(string id);
        Task UnincludeInNetWorth(string id);
        Task CreateAccount(string id, AccountDto accountDto, bool includeInNetWorth);
        Task UpsertAccount(string id, AccountDto accountDto, bool includeInNetWorth);
        Task<Dictionary<string, AccountDto>> GetAccountsIncludedInNetWorth();
        Task<Optional<AccountDto>> TryGetAccount(string id);
        Task<bool> HasAccountWithName(string accountName);
        Task<Dictionary<string, AccountDetails>> GetPagedDetails(int pageSize, int page);
        Task<long> GetNumberOfAccounts();
        Task<int> DeleteAllAccounts();
    }
}
