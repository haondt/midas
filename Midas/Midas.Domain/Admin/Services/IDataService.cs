using Haondt.Core.Models;
using Midas.Domain.Admin.Models;

namespace Midas.Domain.Admin.Services
{
    public interface IDataService
    {
        Task<int> DeleteAllAccounts();
        Task<int> DeleteAllTransactions();
        Result<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId);
        string StartCreateTakeout();
    }
}