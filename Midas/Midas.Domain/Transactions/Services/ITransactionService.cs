using Haondt.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Transactions.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Transactions.Services
{
    public interface ITransactionService
    {
        Task<List<long>> CreateTransactions(List<TransactionDto> transactions);
        Task<(Dictionary<string, decimal> BySource, Dictionary<string, decimal> ByDestination)> GetAmounts(List<TransactionFilter> filters);
        Task<List<string>> GetCategories();
        Task<Dictionary<long, ExtendedTransactionDto>> GetPagedExtendedTransactions(IAccountsService accountsService, List<TransactionFilter> filters, long pageSize, long page);
        Task<Dictionary<long, TransactionDto>> GetPagedTransactions(List<TransactionFilter> filters, long pageSize, long page);
        Task<List<string>> GetTags();
        Task<Dictionary<long, TransactionDto>> GetTransactions(List<TransactionFilter> filters);
        Task<Optional<TransactionDto>> GetTransaction(long transactionId);
        Task<int> DeleteTransactions(List<long> keys);
        Task<bool> DeleteTransaction(long key);
        Task<int> DeleteAllTransactions();
        Task<long> GetTransactionsCount(List<TransactionFilter> filters);
        Task<int> DeleteTransactions(List<TransactionFilter> filters);
        Task<List<long>> ReplaceTransactions(List<TransactionDto> newTransactions, List<long> oldTransacations);
        Task<long> ReplaceTransaction(TransactionDto newTransactions, long oldTransacation, bool keepImportData);
        Task<long> CreateTransaction(TransactionDto transaction);
        Task<Optional<ExtendedTransactionDto>> GetExtendedTransaction(IAccountsService accountsService, long id);
    }
}
