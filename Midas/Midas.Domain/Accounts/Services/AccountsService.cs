using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Midas.Core.Extensions;
using Midas.Domain.Accounts.Models;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Accounts.Services
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
                TransactionFilter.EitherAccountId.Is(id)
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
                TransactionFilter.EitherAccountId.IsOneOf(accounts.Keys.ToList())
            });

            return accounts.ToDictionary(
                kvp => kvp.Key,
                kvp => AccountDetails.FromAccountDto(kvp.Value,
                    amounts.ByDestination.GetValue(kvp.Key).Or(0) - amounts.BySource.GetValue(kvp.Key).Or(0)));
        }

        public Task<Dictionary<string, AccountDto>> GetPagedAccounts(int pageSize, int page)
        {
            var limit = pageSize;
            var offset = (page - 1) * pageSize;
            return accountStorage.GetAll(limit, offset);
        }

        public Task<Optional<AccountDto>> TryGetAccount(string id)
        {
            return accountStorage.TryGet(id);
        }

        public Task<AccountDto> GetAccount(string id)
        {
            return accountStorage.Get(id);
        }

        public Task<Dictionary<string, AccountDto>> GetAccounts(List<string> ids)
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
        public Task CreateAccounts(List<(string Id, AccountDto Account)> accounts)
        {
            return accountStorage.AddMany(accounts);
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

        public async Task<List<string>> GetAccountIdsByName(string name, long? limit = null)
        {
            return await accountStorage.GetAccountIdsByName(name, limit);
        }

        public async Task<Optional<string>> GetAccountIdByName(string name)
        {
            var results = await accountStorage.GetAccountIdsByName(name, 1);
            if (results.Count == 0)
                return new();
            return results[0];
        }

        public Task<long> GetNumberOfAccounts()
        {
            return accountStorage.GetCount();
        }
        public Task<long> GetNumberOfAccountsByPartialName(string partialName)
        {
            return accountStorage.GetCountByPartialName(partialName);
        }

        public Task UpsertAccount(string id, AccountDto accountDto)
        {
            return accountStorage.Set(id, accountDto);
        }
        public Task UpsertAccounts(List<(string Id, AccountDto Account)> accounts)
        {
            return accountStorage.SetMany(accounts);
        }

        public async Task<Dictionary<string, AccountDetails>> SearchPagedDetailsByPartialName(string partialName, int pageSize, int page)
        {
            var limit = pageSize;
            var offset = (page - 1) * pageSize;
            var accountIds = (await accountStorage.SearchAccountsByName(partialName, limit, offset));
            var accounts = await accountStorage.GetMany(accountIds.Select(q => q.Id));
            var netWorthAccounts = await GetAccountsIncludedInNetWorth();

            var amounts = await transactionService.GetAmounts(new List<TransactionFilter>
            {
                TransactionFilter.EitherAccountId.IsOneOf(accounts.Keys.ToList())
            });

            return accounts.ToDictionary(
                kvp => kvp.Key,
                kvp => AccountDetails.FromAccountDto(kvp.Value,
                    amounts.ByDestination.GetValue(kvp.Key).Or(0) - amounts.BySource.GetValue(kvp.Key).Or(0)));
        }
    }
}
