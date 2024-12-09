using SpendLess.Domain.Models;

namespace SpendLess.Transactions.Services
{
    public interface ITransactionService
    {
        Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<TransactionDto> transactions);
        Task<List<bool>> CheckIfTransactionsHaveBeenImported(List<List<string>> sourceDatas);
        Task<List<long>> CreateTransactions(List<TransactionDto> transactions);


    }
}
