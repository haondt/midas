using Haondt.Core.Models;
using SpendLess.Domain.Models;

namespace SpendLess.TransactionImport.Services
{
    public interface ITransactionImportService
    {
        Result<SendToNodeRedResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId);
        string StartDryRun(TransactionImportConfigurationDto configuration, string accountId, List<List<string>> csvData);
    }
}
