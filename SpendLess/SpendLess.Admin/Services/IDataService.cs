using Haondt.Core.Models;

namespace SpendLess.Admin.Services
{
    public interface IDataService
    {
        Result<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId);
        string StartCreateTakeout(bool includeMappings, bool includeAccounts, bool includeTransactions, bool includeFlows);
    }
}