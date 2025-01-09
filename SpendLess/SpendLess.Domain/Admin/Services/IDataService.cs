using Haondt.Core.Models;
using SpendLess.Domain.Admin.Models;

namespace SpendLess.Domain.Admin.Services
{
    public interface IDataService
    {
        Task<int> DeleteAllAccounts();
        Task<int> DeleteAllTransactions();
        Result<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId);
        string StartCreateTakeout();
    }
}