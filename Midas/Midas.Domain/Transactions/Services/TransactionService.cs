using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Persistence.Services;
using Midas.Core.Constants;
using Midas.Core.Extensions;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Transactions.Models;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Transactions.Services
{
    public class TransactionService(ITransactionStorage transactionStorage,
        ITransactionImportDataStorage importDataStorage,
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

            var result = new Dictionary<long, ExtendedTransactionDto>();
            foreach (var (key, value) in transactions)
                result[key] = new ExtendedTransactionDto
                {
                    Amount = value.Amount,
                    DestinationAccount = value.DestinationAccount,
                    DestinationAccountName = accounts.GetValue(value.DestinationAccount).As(k => k.Name).Or(MidasConstants.DefaultAccountName),
                    SourceAccount = value.SourceAccount,
                    SourceAccountName = accounts.GetValue(value.SourceAccount).As(k => k.Name).Or(MidasConstants.DefaultAccountName),
                    Category = value.Category,
                    Description = value.Description,
                    Tags = value.Tags,
                    TimeStamp = value.TimeStamp,
                    SourceDataHashes = (await importDataStorage.Get(key)).Select(q => q.SourceDataHash).ToList()
                };

            return result;
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
                DestinationAccountName = accounts.GetValue(transaction.Value.DestinationAccount).As(k => k.Name).Or(MidasConstants.DefaultAccountName),
                SourceAccount = transaction.Value.SourceAccount,
                SourceAccountName = accounts.GetValue(transaction.Value.SourceAccount).As(k => k.Name).Or(MidasConstants.DefaultAccountName),
                Category = transaction.Value.Category,
                Description = transaction.Value.Description,
                Tags = transaction.Value.Tags,
                TimeStamp = transaction.Value.TimeStamp,
                SourceDataHashes = (await importDataStorage.Get(id)).Select(q => q.SourceDataHash).ToList()
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
