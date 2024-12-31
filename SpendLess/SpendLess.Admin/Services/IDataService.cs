using Haondt.Core.Models;

namespace SpendLess.Admin.Services
{
    public interface IDataService
    {
        Result<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId);
        string StartCreateTakeout();
    }
}