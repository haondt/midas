using Haondt.Persistence.Services;
using SpendLess.Domain.Models;

namespace SpendLess.Transactions.Storages
{
    public interface ITransactionStorage
    {
        Task<List<long>> AddTransactions(List<TransactionDto> transaction);
        Task<TransactionDto> GetTransaction(long key);
        (StorageOperation Operation, Func<List<long>> GetResult) CreateAddTransactionsOperation(List<TransactionDto> transactions);
        Task<List<bool>> CheckIfHasSourceDataHash(IEnumerable<long> hashes);
    }
}
