using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Transactions.Models;

namespace SpendLess.Transactions.Storages
{
    public interface ITransactionStorage
    {
        Task<List<int>> AddTransactions(List<TransactionDto> transaction);
        Task<TransactionDto> GetTransaction(int key);
        (StorageOperation Operation, Func<List<int>> GetResult) CreateAddTransactionsOperation(List<TransactionDto> transactions);
        Task<List<bool>> CheckIfHasSourceDataHash(IEnumerable<long> hashes);
        Task<Dictionary<int, TransactionDto>> GetTransactions(List<TransactionFilter> filters, int? limit = null, int? offset = null);
        Task<int> GetTransactionsCount(List<TransactionFilter> filters);
        Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters);
    }
}
