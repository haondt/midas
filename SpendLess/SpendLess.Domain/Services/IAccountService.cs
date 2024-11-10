using Haondt.Identity.StorageKey;
using SpendLess.Domain.Models;

namespace SpendLess.Domain.Services
{
    public interface IAccountService
    {
        Task<(StorageKey<AccountDto> Id, AccountDto Account)> CreateAccount(string name);
        Task<AccountDto> GetAccount(StorageKey<AccountDto> id);
        Task<Dictionary<StorageKey<AccountDto>, AccountDto>> GetAccounts();
    }
}
