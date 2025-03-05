using Haondt.Core.Models;
using Midas.Domain.Admin.Models;

namespace Midas.Domain.Admin.Services
{
    public interface IDataService
    {
        Task<int> DeleteAllAccounts();
        Task<int> DeleteAllMappings();
        Task<int> DeleteAllTransactions();
        DetailedResult<TakeoutResult, Optional<string>> GetAsyncJobResult(string jobId);
        Task ImportAccounts(TakeoutAccountsDto accounts, bool overwriteExisting);
        Task ImportKvsMappings(TakeoutKvsMappingsDto mappings, bool overwriteExisting);
        Task ImportSupercategories(TakeoutSupercategoriesDto supercategories, bool overwriteExisting);
        Task ImportTransactionImportConfigurations(TakeoutImportConfigurationsDto configurations);
        Task ImportTransactions(TakeoutTransactionsDto transcations);
        string StartCreateTakeout();
    }
}