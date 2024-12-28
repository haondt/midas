using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Transactions.Models;

namespace SpendLess.Transactions.Storages
{
    public interface ITransactionStorage
    {
        Task<List<long>> AddTransactions(List<TransactionDto> transaction);
        Task<TransactionDto> GetTransaction(long key);
        (StorageOperation Operation, Func<List<long>> GetResult) CreateAddTransactionsOperation(List<TransactionDto> transactions);
        Task<List<bool>> CheckIfHasSourceDataHash(IEnumerable<long> hashes);
        Task<Dictionary<long, TransactionDto>> GetTransactions(List<TransactionFilter> filters, long? limit = null, long? offset = null);
        Task<long> GetTransactionsCount(List<TransactionFilter> filters);
        Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters);
        Task<List<string>> GetTags();
        Task<List<string>> GetCategories();
        Task<int> DeleteTransactions(List<long> keys);
        Task<bool> DeleteTransaction(long key);
        Task<int> DeleteTransactions(List<TransactionFilter> filters);
    }
}
