using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Transactions.Storages;

namespace SpendLess.Transactions.Services
{
    public class TransactionService(ITransactionStorage transactionStorage,
        IStorage storage) : ITransactionService
    {
        public async Task<List<long>> CreateTransactions(List<TransactionDto> transactions)
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
