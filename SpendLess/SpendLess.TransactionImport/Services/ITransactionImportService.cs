using Haondt.Core.Models;
using SpendLess.Domain.Models;
using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Services
{
    public interface ITransactionImportService
    {
        Result<SendToNodeRedResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId);
        Result<TransactionImportResultDto, (double Progress, Optional<string> ProgressMessage)> GetImportResult(string jobId);
        string StartDryRun(TransactionImportConfigurationDto configuration, string accountId, List<List<string>> csvData);
        string StartImport(string dryRunId);
    }
}
