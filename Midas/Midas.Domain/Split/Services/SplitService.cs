using Haondt.Core.Extensions;
using Midas.Core.Constants;
using Midas.Core.Extensions;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Split.Models;
using Midas.Domain.Transactions.Models;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Split.Services
{
    public class SplitService(
        ITransactionService transactionService,
        ITransactionImportDataStorage importDataStorage,
        IAccountsService accountsService) : ISplitService
    {
        public async Task<TransactionSplitDryRunResultDto> DryRun(TransactionSplitOptions splitOptions)
        {
            var sourceTransaction = await transactionService.GetTransaction(splitOptions.SourceTransactionId);
            if (!sourceTransaction.HasValue)
                throw new ArgumentException($"Transaction {splitOptions.SourceTransactionId} not found.");

            var accountIds = splitOptions.Splits
                .Select(s => s.SourceAccount)
                .Concat(splitOptions.Splits
                    .Select(s => s.DestinationAccount))
                .Append(sourceTransaction.Value.SourceAccount)
                .Append(sourceTransaction.Value.DestinationAccount)
                .Distinct()
                .ToList();

            var accounts = await accountsService.GetAccounts(accountIds);

            var accountNames = new Dictionary<string, string>();
            foreach (var split in splitOptions.Splits)
            {
                accountNames[split.SourceAccount] = split.SourceAccountName;
                accountNames[split.DestinationAccount] = split.DestinationAccountName;
            }
            foreach (var (id, account) in accounts)
                accountNames[id] = account.Name;
            foreach (var id in accountIds)
                if (!accountNames.ContainsKey(id))
                    accountNames[id] = MidasConstants.FallbackAccountName;

            var newAccounts = accountNames
                .Where(kvp => !accounts.ContainsKey(kvp.Key))
                .ToDictionary();

            var categories = await transactionService.GetCategories();
            var newCategories = splitOptions.Splits.Select(s => s.Category)
                .Where(q => !categories.Contains(q)).ToList();

            var tags = await transactionService.GetTags();
            var newTags = splitOptions.Splits.SelectMany(s => s.Tags)
                .Where(q => !tags.Contains(q)).ToList();

            var balanceChanges = new Dictionary<string, decimal>();
            void includeBalanceChange(string accountId, decimal amount)
            {
                if (!balanceChanges.TryGetValue(accountId, out var currentValue))
                    currentValue = 0;
                balanceChanges[accountId] = currentValue + amount;
            }

            includeBalanceChange(sourceTransaction.Value.SourceAccount, sourceTransaction.Value.Amount);
            includeBalanceChange(sourceTransaction.Value.DestinationAccount, -sourceTransaction.Value.Amount);
            foreach (var split in splitOptions.Splits)
            {
                includeBalanceChange(split.SourceAccount, -split.Amount);
                includeBalanceChange(split.DestinationAccount, split.Amount);
            }

            var result = new TransactionSplitDryRunResultDto
            {
                NewTransactions = splitOptions.Splits,
                NewAccounts = newAccounts,
                NewCategories = newCategories,
                NewTags = newTags,
                BalanceChanges = balanceChanges.Where(kvp => kvp.Value != 0)
                    .Select(kvp =>
                    {
                        var accountName = accountNames[kvp.Key];
                        var isMine = accounts.GetValue(kvp.Key).As(a => a.IsMine).Or(false);
                        return (accountName, isMine, kvp.Value);
                    })
                    .ToList()
            };

            return result;
        }

        public async Task<List<(long Id, ExtendedTransactionDto Transaction)>> PerformSplit(TransactionSplitOptions splitOptions)
        {
            var dryRun = await DryRun(splitOptions);

            await accountsService.CreateAccounts(dryRun.NewAccounts.Select(kvp => (kvp.Key, new AccountDto
            {
                IsMine = false,
                Name = kvp.Key,
            })).ToList());

            var newIds = await transactionService.ReplaceTransactions(
                dryRun.NewTransactions.Select(q => q.AsTransaction()).ToList(),
                [splitOptions.SourceTransactionId]);

            var newSourceData = newIds
                .Zip(dryRun.NewTransactions)
                .SelectMany(q => q.Second.ImportDatum
                    .Select(r => r.AsImportDataDto(q.First)))
                .ToList();
            await importDataStorage.AddMany(newSourceData);

            return newIds
                .Zip(dryRun.NewTransactions)
                .Select(q => (q.First, q.Second.AsExtendedTransaction()))
                .ToList();
        }
    }
}
