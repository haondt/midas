using Haondt.Core.Extensions;
using Haondt.Core.Models;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Accounts.Models;
using SpendLess.Domain.Transactions.Services;
using SpendLess.Persistence.Models;
using SpendLess.Persistence.Storages.Abstractions;

namespace SpendLess.Domain.Accounts.Services
{
    public class AccountsService(
        IAccountStorage accountStorage,
        ITransactionService transactionService) : IAccountsService
    {
        public async Task<AccountDetails> GetDetails(string id)
        {
            var account = await accountStorage.TryGet(id);
            var amounts = await transactionService.GetAmounts(new()
            {
                TransactionFilter.EitherAccountIs(id)
            });
            var balance = amounts.ByDestination.GetValue(id).Or(0) - amounts.BySource.GetValue(id).Or(0);

            return AccountDetails.FromAccountDto(account, balance);
        }

        public async Task<Dictionary<string, AccountDetails>> GetPagedDetails(int pageSize, int page)
        {
            var limit = pageSize;
            var offset = (page - 1) * pageSize;
            var accounts = await accountStorage.GetAll(limit, offset);
            var netWorthAccounts = await GetAccountsIncludedInNetWorth();

            var amounts = await transactionService.GetAmounts(new List<TransactionFilter>
            {
                TransactionFilter.EitherAccountIsOneOf(accounts.Keys.ToList())
            });

            return accounts.ToDictionary(
                kvp => kvp.Key,
                kvp => AccountDetails.FromAccountDto(kvp.Value,
                    amounts.ByDestination.GetValue(kvp.Key).Or(0) - amounts.BySource.GetValue(kvp.Key).Or(0)));
        }

        public Task<Optional<AccountDto>> TryGetAccount(string id)
        {
            return accountStorage.TryGet(id);
        }

        public Task<AccountDto> GetAccount(string id)
        {
            return accountStorage.Get(id);
        }
        public Task<Dictionary<string, AccountDto>> GetMany(List<string> ids)
        {
            return accountStorage.GetMany(ids);
        }

        public Task<bool> HasAccountWithName(string accountName)
        {
            return accountStorage.HasAccountWithName(accountName);
        }

        public Task<Dictionary<string, AccountDto>> GetAccountsIncludedInNetWorth()
        {
            return accountStorage.GetAllMine();
        }

        public Task CreateAccount(string id, AccountDto accountDto)
        {
            return accountStorage.Add(id, accountDto);
        }

        public Task DeleteAccount(string id)
        {
            return accountStorage.Delete(id);
        }
        public Task<int> DeleteAllAccounts()
        {
            return accountStorage.DeleteAll();
        }
        public async Task<List<(string Name, string Id)>> SearchAccountsByName(string partialName)
        {
            return await accountStorage.SearchAccountsByName(partialName, 5); // todo: appsettings
        }

        public async Task<List<string>> GetAccountIdsByName(string name)
        {
            return await accountStorage.GetAccountIdsByName(name);
        }

        public Task<long> GetNumberOfAccounts()
        {
            return accountStorage.GetCount();
        }

        public Task UpsertAccount(string id, AccountDto accountDto)
        {
            return accountStorage.Set(id, accountDto);
        }

    }
}
