using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using SpendLess.Accounts.Models;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Services;

namespace SpendLess.Accounts.Services
{
    public class AccountsService(ISingleTypeSpendLessStorage<AccountDto> ownedAccountsStorage, ITransactionService transactionService) : IAccountsService
    {
        public async Task Disown(string id)
        {
            var accountKey = id.SeedStorageKey<AccountDto>();
            await ownedAccountsStorage.Delete(accountKey);
        }

        public async Task<OwnedAccountDetails> GetDetails(string id)
        {
            var accountKey = id.SeedStorageKey<AccountDto>();
            var dto = await ownedAccountsStorage.Get(accountKey);
            var amounts = await transactionService.GetAmounts(new()
            {
                TransactionFilter.EitherAccountIs(id)
            });

            return new OwnedAccountDetails
            {
                Name = dto.Name,
                Balance = amounts.ByDestination.GetValue(id).Or(0) - amounts.BySource.GetValue(id).Or(0)
            };
        }

        public async Task<Dictionary<string, AccountDto>> GetOwned()
        {
            var accounts = await ownedAccountsStorage.GetAll();
            return accounts.ToDictionary(kvp => kvp.Key.SingleValue(), kvp => kvp.Value);
        }


        public Task<Optional<AccountDto>> TryGetOwnedAccount(string id)
        {
            return ownedAccountsStorage.TryGet(id.SeedStorageKey<AccountDto>());
        }

        public Task UpsertOwnedAccount(string id, AccountDto ownedAccountDto)
        {
            return ownedAccountsStorage.Set(id.SeedStorageKey<AccountDto>(), ownedAccountDto, [AccountDto.GetNameForeignKey(ownedAccountDto.Name)]);
        }
    }
}
