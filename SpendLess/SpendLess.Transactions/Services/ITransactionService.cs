using SpendLess.Domain.Models;
using SpendLess.Transactions.Models;

namespace SpendLess.Transactions.Services
{
    public interface ITransactionService
    {
        Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<TransactionDto> transactions);
        Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<List<string>> sourceDatas);
        Task<List<int>> CreateTransactions(List<TransactionDto> transactions);
        Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters);
        Task<Dictionary<int, ExtendedTransactionDto>> GetPagedExtendedTransactions(List<TransactionFilter> filters, int pageSize, int page);
        Task<Dictionary<int, TransactionDto>> GetPagedTransactions(List<TransactionFilter> filters, int pageSize, int page);
        Task<Dictionary<int, TransactionDto>> GetTransactions(List<TransactionFilter> filters);
        Task<int> GetTransactionsCount(List<TransactionFilter> filters);
    }
}
