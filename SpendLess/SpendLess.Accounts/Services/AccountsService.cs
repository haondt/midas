using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using SpendLess.Accounts.Models;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Services;

namespace SpendLess.Accounts.Services
{
    public class AccountsService(
        IStorage storage,
        ITransactionService transactionService) : IAccountsService
    {
        public async Task IncludeInNetWorth(string id)
        {
            var accountKey = id.SeedStorageKey<AccountDto>();
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new AddForeignKeyOperation
                {
                    ForeignKey = IncludeInNetWorthDto.GetForeignKey(),
                    Target = accountKey
                }
            });
        }

        public async Task UnincludeInNetWorth(string id)
        {
            var accountKey = id.SeedStorageKey<AccountDto>();
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new RemoveForeignKeyOperation
                {
                    ForeignKey = IncludeInNetWorthDto.GetForeignKey(),
                    Target = accountKey
                }
            });
        }

        public async Task<List<bool>> IsIncludedInNetWorth(List<string> ids)
        {
            var includedAccounts = await storage.GetManyByForeignKey(IncludeInNetWorthDto.GetForeignKey());
            var idSet = includedAccounts
                .Select(q => q.Key.SingleValue())
                .ToHashSet();

            return ids.Select(id => idSet.Contains(id)).ToList();
        }

        public async Task<bool> IsIncludedInNetWorth(string id) => (await IsIncludedInNetWorth([id]))[0];


        public async Task<AccountDetails> GetDetails(string id)
        {
            var accountKey = id.SeedStorageKey<AccountDto>();
            var dto = await storage.Get(accountKey);
            var amounts = await transactionService.GetAmounts(new()
            {
                TransactionFilter.EitherAccountIs(id)
            });

            return new AccountDetails
            {
                Name = dto.AsOptional().As(q => q.Name).Or(SpendLessConstants.DefaultAccountName),
                IsIncludedInNetWorth = await IsIncludedInNetWorth(id),
                Balance = amounts.ByDestination.GetValue(id).Or(0) - amounts.BySource.GetValue(id).Or(0)
            };
        }

        public async Task<Dictionary<string, AccountDetails>> GetPagedDetails(int pageSize, int page)
        {
            var limit = pageSize;
            var offset = (page - 1) * pageSize;
            var accounts = (await storage.GetManyByForeignKey(StorageKey<AccountDto>.Empty, limit, offset))
                .ToDictionary(q => q.Key.SingleValue(), q => q.Value);

            var netWorthAccounts = await GetAccountsIncludedInNetWorth();

            var amounts = await transactionService.GetAmounts(new List<TransactionFilter>
            {
                TransactionFilter.EitherAccountIsOneOf(accounts.Keys.ToList())
            });

            return accounts.ToDictionary(
                kvp => kvp.Key,
                kvp => new AccountDetails
                {
                    Name = kvp.Value.Name,
                    Balance = amounts.ByDestination.GetValue(kvp.Key).Or(0) - amounts.BySource.GetValue(kvp.Key).Or(0),
                    IsIncludedInNetWorth = netWorthAccounts.ContainsKey(kvp.Key)
                });
        }

        public async Task<Optional<AccountDto>> TryGetAccount(string id)
        {
            var result = await storage.Get(id.SeedStorageKey<AccountDto>());
            return result.AsOptional();
        }

        public async Task<AccountDto> GetAccount(string id)
        {

            var result = await TryGetAccount(id);
            if (!result.HasValue)
                throw new KeyNotFoundException($"No such account with id {id}");
            return result.Value;
        }

        public async Task<bool> HasAccountWithName(string accountName)
        {
            return (await storage.GetManyByForeignKey(AccountNameDto.GetForeignKey(accountName))).Count > 0;
        }

        public async Task<Dictionary<string, AccountDto>> GetAccountsIncludedInNetWorth()
        {
            var accounts = await storage.GetManyByForeignKey(IncludeInNetWorthDto.GetForeignKey());
            return accounts.ToDictionary(kvp => kvp.Key.SingleValue(), kvp => kvp.Value);
        }

        public async Task CreateAccount(string id, AccountDto accountDto, bool includeInNetWorth)
        {
            var key = id.SeedStorageKey<AccountDto>();
            var foreignKeys = new List<StorageKey<AccountDto>>
            {
                StorageKey<AccountDto>.Empty,
                AccountNameDto.GetForeignKey(accountDto.Name)
            };
            if (includeInNetWorth)
                foreignKeys.Add(IncludeInNetWorthDto.GetForeignKey());
            await storage.Add(key, accountDto, foreignKeys);
        }

        public Task DeleteAccount(string id)
        {
            var key = id.SeedStorageKey<AccountDto>();
            return storage.Delete(key);
        }
        public Task<int> DeleteAllAccounts()
        {
            return storage.DeleteByForeignKey(StorageKey<AccountDto>.Empty);
        }

        public Task<long> GetNumberOfAccounts()
        {
            return storage.CountManyByForeignKey(StorageKey<AccountDto>.Empty);
        }

        public async Task UpsertAccount(string id, AccountDto accountDto, bool includeInNetWorth)
        {
            var key = id.SeedStorageKey<AccountDto>();
            var existingAccount = await storage.Get(key);

            var operations = new List<StorageOperation>
            {
                new SetOperation { Target = key, Value = accountDto },
                new AddForeignKeyOperation { Target = key, ForeignKey = StorageKey<AccountDto>.Empty }
            };

            if (includeInNetWorth)
                operations.Add(new AddForeignKeyOperation { Target = key, ForeignKey = IncludeInNetWorthDto.GetForeignKey() });
            else
                operations.Add(new RemoveForeignKeyOperation { Target = key, ForeignKey = IncludeInNetWorthDto.GetForeignKey() });

            if (existingAccount.IsSuccessful)
            {
                if (existingAccount.Value.Name != accountDto.Name)
                {
                    operations.Add(new RemoveForeignKeyOperation { Target = key, ForeignKey = AccountNameDto.GetForeignKey(existingAccount.Value.Name) });
                    operations.Add(new AddForeignKeyOperation { Target = key, ForeignKey = AccountNameDto.GetForeignKey(accountDto.Name) });
                }
            }
            else
            {
                operations.Add(new AddForeignKeyOperation { Target = key, ForeignKey = AccountNameDto.GetForeignKey(accountDto.Name) });
            }

            await storage.PerformTransactionalBatch(operations);
        }

    }
}
