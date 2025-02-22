﻿using Haondt.Core.Models;
using Haondt.Persistence.Services;
using Midas.Persistence.Models;

namespace Midas.Persistence.Storages.Abstractions
{
    public interface ITransactionStorage
    {
        Task<List<long>> AddTransactions(List<TransactionDto> transactions, List<long>? deleteTransactions = null);
        Task<Optional<TransactionDto>> GetTransaction(long key);
        (StorageOperation Operation, Func<List<long>> GetResult) CreateAddTransactionsOperation(List<TransactionDto> transactions);
        Task<Dictionary<long, TransactionDto>> GetTransactions(List<TransactionFilter> filters, long? limit = null, long? offset = null);
        Task<long> GetTransactionsCount(List<TransactionFilter> filters);
        Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters);
        Task<List<string>> GetTags();
        Task<List<string>> GetCategories();
        Task<int> DeleteTransactions(List<long> keys);
        Task<bool> DeleteTransaction(long key);
        Task<int> DeleteTransactions(List<TransactionFilter> filters);
        Task<int> DeleteAllTransactions();
    }
}
