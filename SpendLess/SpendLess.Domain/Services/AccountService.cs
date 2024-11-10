using Haondt.Identity.StorageKey;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Exceptions;
using SpendLess.Persistence.Services;

namespace SpendLess.Domain.Services
{
    public class AccountService(ISpendLessStorage storage) : IAccountService
    {
        public async Task<Dictionary<StorageKey<AccountDto>, AccountDto>> GetAccounts()
        {
            var models = await storage.GetMany(AccountDto.GetStorageKey(Guid.Empty));
            return models.ToDictionary(t => t.Key, t => t.Value);
        }

        public async Task<(StorageKey<AccountDto> Id, AccountDto Account)> CreateAccount(string name)
        {
            var key = AccountDto.GetStorageKey(Guid.NewGuid());
            var value = new AccountDto
            {
                Name = name,
                Balance = 0
            };
            await storage.Set(key, value);
            return (key, value);
        }

        public async Task<AccountDto> GetAccount(StorageKey<AccountDto> id)
        {
            var result = await storage.Get(id);
            if (!result.IsSuccessful)
                throw new StorageException($"Couldn't find account with id {id}");
            return result.Value;
        }
    }
}
