using Haondt.Persistence.Services;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Storages;

namespace SpendLess.Transactions.Services
{
    public class TransactionService(ITransactionStorage transactionStorage,
        ISingleTypeSpendLessStorage<AccountDto> accountStorage,
        IStorage storage) : ITransactionService
    {
        public async Task<List<int>> CreateTransactions(List<TransactionDto> transactions)
        {
            if (transactions.Count == 0)
                return [];

            var applicationState = await storage.GetDefault<SpendLessStateDto>(SpendLessStateDto.StorageKey);
            var originalCategoryCount = applicationState.Categories.Count;
            var originalTagCount = applicationState.Tags.Count;
            foreach (var transaction in transactions)
            {
                applicationState.Categories.Add(transaction.Category);
                foreach (var tag in transaction.Tags)
                    applicationState.Tags.Add(tag);
            }

            var operations = new List<StorageOperation>();

            if (applicationState.Categories.Count > originalCategoryCount || applicationState.Tags.Count > originalTagCount)
                operations.Add(new SetOperation
                {
                    Target = SpendLessStateDto.StorageKey,
                    Value = applicationState
                });

            var (addTransactionsOperation, getResult) = transactionStorage.CreateAddTransactionsOperation(transactions);
            operations.Add(addTransactionsOperation);

            await storage.PerformTransactionalBatch(operations);

            return getResult();
        }

        public Task<Dictionary<int, TransactionDto>> GetTransactions(List<TransactionFilter> filters)
        {
            return transactionStorage.GetTransactions(filters);
        }
        public Task<int> GetTransactionsCount(List<TransactionFilter> filters)
        {
            return transactionStorage.GetTransactionsCount(filters);
        }

        public Task<Dictionary<int, TransactionDto>> GetPagedTransactions(
            List<TransactionFilter> filters,
            int pageSize,
            int page)
        {
            return transactionStorage.GetTransactions(filters, pageSize, (page - 1) * pageSize);
        }

        public async Task<Dictionary<int, ExtendedTransactionDto>> GetPagedExtendedTransactions(
            List<TransactionFilter> filters,
            int pageSize,
            int page)
        {
            var transactions = await GetPagedTransactions(filters, pageSize, page);
            var accountIds = transactions.Values.SelectMany(t => new List<string> { t.SourceAccount, t.DestinationAccount })
                .Distinct();
            var accountKeys = accountIds
                .Select(k => k.SeedStorageKey<AccountDto>())
                .ToList();

            var accountNames = (await accountStorage.TryGetMany(accountKeys))
                .Zip(accountIds)
                .Where(q => q.First.HasValue)
                .ToDictionary(q => q.Second, q => q.First.Value.Name);

            return transactions.ToDictionary(kvp => kvp.Key,
                kvp => new ExtendedTransactionDto
                {
                    Amount = kvp.Value.Amount,
                    DestinationAccount = kvp.Value.DestinationAccount,
                    DestinationAccountName = accountNames.GetValueOrDefault(kvp.Value.DestinationAccount, SpendLessConstants.DefaultAccountName),
                    ImportAccount = kvp.Value.ImportAccount,
                    SourceAccount = kvp.Value.SourceAccount,
                    SourceAccountName = accountNames.GetValueOrDefault(kvp.Value.SourceAccount, SpendLessConstants.DefaultAccountName),
                    Category = kvp.Value.Category,
                    Description = kvp.Value.Description,
                    SourceData = kvp.Value.SourceData,
                    Tags = kvp.Value.Tags,
                    TimeStamp = kvp.Value.TimeStamp,
                });
        }

        public Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters)
        {
            return transactionStorage.GetAmounts(filters);
        }

        public Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<TransactionDto> transactions)
        {
            return transactionStorage.CheckIfHasSourceDataHash(transactions.Select(t => t.SourceDataHash));
        }

        public Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<List<string>> sourceDatas)
        {
            return transactionStorage.CheckIfHasSourceDataHash(sourceDatas.Select(TransactionDto.HashSourceData));
        }
    }
}
