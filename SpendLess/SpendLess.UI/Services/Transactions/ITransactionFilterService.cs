using SpendLess.Persistence.Models;

namespace SpendLess.UI.Services.Transactions
{
    public interface ITransactionFilterService
    {
        Task<IEnumerable<TransactionFilter>> ParseFiltersAsync(IEnumerable<string> filters);
    }
}
