using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Midas.Core.Constants;
using Midas.Core.Extensions;
using Midas.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Merge.Models;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Merge.Services
{
    public class MergeService(ITransactionService transactionService,
        ITransactionImportDataStorage importDataStorage,
        IAccountsService accountsService) : IMergeService
    {
        public async Task<TransactionMergeDefaultValuesDto> CreateDefaults(List<TransactionFilter> filters)
        {
            var transactions = await transactionService.GetTransactions(filters);

            if (transactions.Count == 0)
                return new TransactionMergeDefaultValuesDto
                {
                    SourceAccount = (MidasConstants.DefaultAccountName, Guid.NewGuid().ToString()),
                    DestinationAccount = (MidasConstants.DefaultAccountName, Guid.NewGuid().ToString()),
                    MeanAmount = 0,
                    Amount = 0,
                    SumAmount = 0,
                    Category = MidasConstants.DefaultCategory,
                    ConcatenatedDescription = "",
                    Timestamp = AbsoluteDateTime.Now,
                    CurrentTimestamp = AbsoluteDateTime.Now,
                    FirstTimestamp = AbsoluteDateTime.Now,
                    LastTimestamp = AbsoluteDateTime.Now,
                    MeanTimestamp = AbsoluteDateTime.Now,
                    Description = "",
                    Tags = [],
                };

            var accounts = transactions
                .SelectMany(t => new string[] { t.Value.SourceAccount, t.Value.DestinationAccount })
                .Distinct();

            var sourceAccounts = transactions
                .Select(t => t.Value.SourceAccount)
                .GroupBy(t => t)
                .ToDictionary(grp => grp.Key, grp => grp.Count());

            var destinationAccounts = transactions
                .Select(t => t.Value.DestinationAccount)
                .GroupBy(t => t)
                .ToDictionary(grp => grp.Key, grp => grp.Count());

            var leastSourcedAccount = accounts
                .OrderBy(a => sourceAccounts.GetValueOrDefault(a, 0))
                .First();

            var leastDestinedAccount = accounts
                .OrderBy(a => destinationAccounts.GetValueOrDefault(a, 0))
                .First();

            var sourceAccount = leastDestinedAccount;
            var destinationAccount = leastSourcedAccount;
            var sourceAccountDto = await accountsService.TryGetAccount(sourceAccount);
            var destinationAccountDto = await accountsService.TryGetAccount(destinationAccount);

            var amounts = transactions
                .Select(s => s.Value.Amount);

            var topCategory = transactions
                .GroupBy(t => t.Value.Category)
                .OrderByDescending(grp => grp.Count())
                .First()
                .Key;

            var concatenatedDescription = string.Join(" | ", transactions
                .Select(t => t.Value.Description)
                .Distinct());
            concatenatedDescription = concatenatedDescription.Substring(0, Math.Min(concatenatedDescription.Length, 100)); // todo: appsettings

            var timestamps = transactions.Select(t => t.Value.TimeStamp);
            var meanTimestamp = new AbsoluteDateTime((long)timestamps.Select(t => t.UnixTimeSeconds).Average());

            return new TransactionMergeDefaultValuesDto
            {
                SourceAccount = sourceAccountDto
                    .As(a => (a.Name, sourceAccount))
                    .Or((MidasConstants.DefaultAccountName, MidasConstants.DefaultAccount)),
                DestinationAccount = destinationAccountDto
                    .As(a => (a.Name, destinationAccount))
                    .Or((MidasConstants.DefaultAccountName, MidasConstants.DefaultAccount)),
                MeanAmount = Math.Round(amounts.Average(), 2),
                SumAmount = amounts.Sum(),
                Amount = Math.Round((
                    transactions
                        .Where(t => t.Value.SourceAccount == sourceAccount)
                        .Sum(t => t.Value.Amount)
                    + transactions
                        .Where(t => t.Value.DestinationAccount == destinationAccount)
                        .Sum(t => t.Value.Amount)) / 2, 2),
                Category = topCategory,
                Description = concatenatedDescription,
                ConcatenatedDescription = concatenatedDescription,
                Timestamp = meanTimestamp,
                CurrentTimestamp = AbsoluteDateTime.Now,
                FirstTimestamp = timestamps.Min(),
                LastTimestamp = timestamps.Max(),
                MeanTimestamp = meanTimestamp,
                Tags = transactions.SelectMany(t => t.Value.Tags).Distinct().ToList()
            };
        }

        public async Task<TransactionMergeDryRunResultDto> DryRun(TransactionMergeDryRunOptions options)
        {
            var newAccounts = new Dictionary<string, string>();

            var sourceAccountId = options.SourceAccountId;
            var existingSourceAccount = await accountsService.TryGetAccount(sourceAccountId);
            string sourceAccountName;
            bool sourceAccountIsMine = false;
            if (existingSourceAccount.HasValue)
            {
                sourceAccountName = existingSourceAccount.Value.Name;
                sourceAccountIsMine = existingSourceAccount.Value.IsMine;
            }
            else
            {
                sourceAccountName = options.PreferredSourceAccountName.Or(MidasConstants.DefaultAccountName);
                newAccounts[sourceAccountId] = sourceAccountName;
            }

            var destinationAccountId = options.DestinationAccountId;
            var existingDestinationAccount = await accountsService.TryGetAccount(destinationAccountId);
            string destinationAccountName;
            bool destinationAccountIsMine = false;
            if (existingDestinationAccount.HasValue)
            {
                destinationAccountName = existingDestinationAccount.Value.Name;
                destinationAccountIsMine = existingDestinationAccount.Value.IsMine;
            }
            else
            {
                destinationAccountName = options.PreferredDestinationAccountName.Or(MidasConstants.DefaultAccountName);
                newAccounts[destinationAccountId] = destinationAccountName;
            }

            var categoryIsNew = !(await transactionService.GetCategories()).Contains(options.Category);

            var existingTags = await transactionService.GetTags();
            var newTags = options.Tags.Where(t => !existingTags.Contains(t)).ToList();

            var amounts = await transactionService.GetAmounts(options.TargetTransactionFilters);
            var balanceChanges = new Dictionary<string, decimal>();
            foreach (var (account, amount) in amounts.BySource)
            {
                if (balanceChanges.TryGetValue(account, out var currentAmount))
                    balanceChanges[account] = currentAmount + amount;
                else
                    balanceChanges[account] = amount;
            }
            foreach (var (account, amount) in amounts.ByDestination)
            {
                if (balanceChanges.TryGetValue(account, out var currentAmount))
                    balanceChanges[account] = currentAmount - amount;
                else
                    balanceChanges[account] = -amount;
            }
            if (balanceChanges.TryGetValue(sourceAccountId, out var currentSourceAmount))
                balanceChanges[sourceAccountId] = currentSourceAmount - options.Amount;
            else
                balanceChanges[sourceAccountId] = -options.Amount;
            if (balanceChanges.TryGetValue(destinationAccountId, out var currentDestinationAmount))
                balanceChanges[destinationAccountId] = currentDestinationAmount + options.Amount;
            else
                balanceChanges[destinationAccountId] = options.Amount;
            var accountInfo = new Dictionary<string, (string Name, bool IsMine)>
            {
                {sourceAccountId,  (sourceAccountName, sourceAccountIsMine) },
                {destinationAccountId, (destinationAccountName, destinationAccountIsMine)}
            };
            foreach (var accountId in amounts.BySource.Select(kvp => kvp.Key)
                .Concat(amounts.ByDestination.Select(kvp => kvp.Key))
                .Distinct())
            {
                if (accountInfo.ContainsKey(accountId))
                    continue;
                if ((await accountsService.TryGetAccount(accountId)).Test(out var foundAccount))
                    accountInfo[accountId] = (foundAccount.Name, foundAccount.IsMine);
                else
                    accountInfo[accountId] = (MidasConstants.DefaultAccountName, false);
            }
            var balanceChangesList = balanceChanges
                .Where(kvp => kvp.Value != 0)
                .Select(kvp =>
                {
                    var (name, isMine) = accountInfo[kvp.Key];
                    return (name, isMine, kvp.Value);
                })
                .ToList();

            return new()
            {
                NewAccounts = newAccounts,
                NewCategory = categoryIsNew ? options.Category : new Optional<string>(),
                NewTags = newTags,
                BalanceChanges = balanceChangesList,
                Amount = options.Amount,
                Tags = options.Tags.Distinct().ToList(),
                Category = options.Category,
                SourceAccount = (sourceAccountId, sourceAccountName),
                DestinationAccount = (destinationAccountId, destinationAccountName),
                Description = options.Description,
                Timestamp = options.Timestamp
            };
        }

        public async Task<long> PerformMerge(TransactionMergeDryRunOptions options)
        {
            var dryRunResult = await DryRun(options);

            foreach (var account in dryRunResult.NewAccounts)
                await accountsService.CreateAccount(account.Key, new AccountDto
                {
                    IsMine = false,
                    Name = account.Value
                });

            // todo: make it one transaction
            var oldIds = (await transactionService.GetTransactions(options.TargetTransactionFilters)).Keys.ToList();
            var oldImportData = (await importDataStorage.GetMany(oldIds)).SelectMany(q => q).ToList();
            var newIds = await transactionService.ReplaceTransactions([new TransactionDto {
                Amount = dryRunResult.Amount,
                DestinationAccount = dryRunResult.DestinationAccount.Id,
                SourceAccount = dryRunResult.SourceAccount.Id,
                Category = dryRunResult.Category,
                Description  = dryRunResult.Description,
                Tags = dryRunResult.Tags.ToHashSet(),
                TimeStamp = dryRunResult.Timestamp
            }], oldIds);
            var newId = newIds[0];
            await importDataStorage.AddMany(oldImportData.Select(q => new TransactionImportDataDto
            {
                Account = q.Account,
                ConfigurationSlug = q.ConfigurationSlug,
                SourceData = q.SourceData,
                Transaction = newId
            }));

            return newId;
        }
    }
}
