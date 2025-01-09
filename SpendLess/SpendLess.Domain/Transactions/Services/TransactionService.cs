using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Persistence.Services;
using SpendLess.Core.Constants;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Accounts.Services;
using SpendLess.Domain.Transactions.Models;
using SpendLess.Persistence.Models;
using SpendLess.Persistence.Storages.Abstractions;

namespace SpendLess.Domain.Transactions.Services
{
    public class TransactionService(ITransactionStorage transactionStorage,
        IStorage storage) : ITransactionService
    {
        public async Task<long> CreateTransaction(TransactionDto transaction)
        {
            return (await CreateTransactions([transaction]))[0];
        }

        public async Task<List<long>> CreateTransactions(List<TransactionDto> transactions)
        {
            if (transactions.Count == 0)
                return [];


            var operations = new List<StorageOperation>();
            var (addTransactionsOperation, getResult) = transactionStorage.CreateAddTransactionsOperation(transactions);
            operations.Add(addTransactionsOperation);

            await storage.PerformTransactionalBatch(operations);

            return getResult();
        }

        public Task<List<long>> ReplaceTransactions(List<TransactionDto> newTransactions, List<long> oldTransacations)
        {
            return transactionStorage.AddTransactions(newTransactions, oldTransacations);
        }


        public Task<Dictionary<long, TransactionDto>> GetTransactions(List<TransactionFilter> filters)
        {
            return transactionStorage.GetTransactions(filters);
        }
        public Task<long> GetTransactionsCount(List<TransactionFilter> filters)
        {
            return transactionStorage.GetTransactionsCount(filters);
        }

        public Task<Dictionary<long, TransactionDto>> GetPagedTransactions(
            List<TransactionFilter> filters,
            long pageSize,
            long page)
        {
            return transactionStorage.GetTransactions(filters, pageSize, (page - 1) * pageSize);
        }

        public async Task<Dictionary<long, ExtendedTransactionDto>> GetPagedExtendedTransactions(
            IAccountsService accountsService,
            List<TransactionFilter> filters,
            long pageSize,
            long page)
        {
            var transactions = await GetPagedTransactions(filters, pageSize, page);
            var accountIds = transactions.Values.SelectMany(t => new List<string> { t.SourceAccount, t.DestinationAccount })
                .Distinct();
            var accounts = (await accountsService.GetMany(accountIds.ToList()));

            return transactions.ToDictionary(kvp => kvp.Key,
                kvp => new ExtendedTransactionDto
                {
                    Amount = kvp.Value.Amount,
                    DestinationAccount = kvp.Value.DestinationAccount,
                    DestinationAccountName = accounts.GetValue(kvp.Value.DestinationAccount).As(k => k.Name).Or(SpendLessConstants.DefaultAccountName),
                    SourceAccount = kvp.Value.SourceAccount,
                    SourceAccountName = accounts.GetValue(kvp.Value.SourceAccount).As(k => k.Name).Or(SpendLessConstants.DefaultAccountName),
                    Category = kvp.Value.Category,
                    Description = kvp.Value.Description,
                    Tags = kvp.Value.Tags,
                    TimeStamp = kvp.Value.TimeStamp,
                });
        }

        public Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters)
        {
            return transactionStorage.GetAmounts(filters);
        }

        public async Task<Optional<ExtendedTransactionDto>> GetExtendedTransaction(
            IAccountsService accountsService,
            long id)
        {
            var transaction = await transactionStorage.GetTransaction(id);
            if (!transaction.HasValue)
                return new();

            var accounts = await (transaction.Value.SourceAccount == transaction.Value.DestinationAccount
                ? accountsService.GetMany([transaction.Value.DestinationAccount])
                : accountsService.GetMany([transaction.Value.SourceAccount, transaction.Value.DestinationAccount]));


            return new ExtendedTransactionDto
            {
                Amount = transaction.Value.Amount,
                DestinationAccount = transaction.Value.DestinationAccount,
                DestinationAccountName = accounts.GetValue(transaction.Value.DestinationAccount).As(k => k.Name).Or(SpendLessConstants.DefaultAccountName),
                SourceAccount = transaction.Value.SourceAccount,
                SourceAccountName = accounts.GetValue(transaction.Value.SourceAccount).As(k => k.Name).Or(SpendLessConstants.DefaultAccountName),
                Category = transaction.Value.Category,
                Description = transaction.Value.Description,
                Tags = transaction.Value.Tags,
                TimeStamp = transaction.Value.TimeStamp,
            };
        }


        public Task<List<string>> GetCategories()
        {
            return transactionStorage.GetCategories();
        }
        public Task<List<string>> GetTags()
        {
            return transactionStorage.GetTags();
        }

        public Task<int> DeleteTransactions(List<long> keys)
        {
            return transactionStorage.DeleteTransactions(keys);
        }

        public Task<int> DeleteAllTransactions()
        {
            return transactionStorage.DeleteAllTransactions();
        }

        public Task<bool> DeleteTransaction(long key)
        {
            return transactionStorage.DeleteTransaction(key);
        }
        public Task<int> DeleteTransactions(List<TransactionFilter> filters)
        {
            return transactionStorage.DeleteTransactions(filters);
        }
    }
}
