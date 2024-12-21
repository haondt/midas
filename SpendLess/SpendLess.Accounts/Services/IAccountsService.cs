using Haondt.Core.Models;
using SpendLess.Accounts.Models;
using SpendLess.Domain.Models;

namespace SpendLess.Accounts.Services
{
    public interface IAccountsService
    {
        Task Disown(string id);
        Task<Dictionary<string, AccountDto>> GetOwned();
        Task<OwnedAccountDetails> GetDetails(string id);
        Task<Optional<AccountDto>> TryGetOwnedAccount(string id);
        Task UpsertOwnedAccount(string id, AccountDto ownedAccountDto);
    }
}
