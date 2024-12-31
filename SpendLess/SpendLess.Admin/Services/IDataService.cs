using Haondt.Core.Models;

namespace SpendLess.Admin.Services
{
    public interface IDataService
    {
        Task<int> DeleteAllAccounts();
        Task<int> DeleteAllTransactions();
        Result<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId);
        string StartCreateTakeout();
    }
}