using SpendLess.Domain.Models;
using SpendLess.Transactions.Models;

namespace SpendLess.Transactions.Services
{
    public interface ITransactionService
    {
        Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<TransactionDto> transactions);
        Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<List<string>> sourceDatas);
        Task<List<long>> CreateTransactions(List<TransactionDto> transactions);
        Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters);
        Task<List<string>> GetCategories();
        Task<Dictionary<long, ExtendedTransactionDto>> GetPagedExtendedTransactions(List<TransactionFilter> filters, long pageSize, long page);
        Task<Dictionary<long, TransactionDto>> GetPagedTransactions(List<TransactionFilter> filters, long pageSize, long page);
        Task<List<string>> GetTags();
        Task<Dictionary<long, TransactionDto>> GetTransactions(List<TransactionFilter> filters);
        Task<int> DeleteTransactions(List<long> keys);
        Task<bool> DeleteTransaction(long key);
        Task<long> GetTransactionsCount(List<TransactionFilter> filters);
        Task<int> DeleteTransactions(List<TransactionFilter> filters);
    }
}
