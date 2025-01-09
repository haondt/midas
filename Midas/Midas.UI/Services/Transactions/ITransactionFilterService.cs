using Midas.Persistence.Models;

namespace Midas.UI.Services.Transactions
{
    public interface ITransactionFilterService
    {
        Task<IEnumerable<TransactionFilter>> ParseFiltersAsync(IEnumerable<string> filters);
    }
}
