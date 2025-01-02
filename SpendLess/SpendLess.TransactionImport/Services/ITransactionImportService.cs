using Haondt.Core.Models;
using SpendLess.Domain.Models;
using SpendLess.TransactionImport.Models;
using SpendLess.Transactions.Models;

namespace SpendLess.TransactionImport.Services
{
    public interface ITransactionImportService
    {
        Result<DryRunResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId);
        Result<TransactionImportResultDto, (double Progress, Optional<string> ProgressMessage)> GetImportResult(string jobId);
        string StartDryRun(List<TransactionFilter> filters, bool addImportTag, string? configurationSlug = null, string? accountId = null);
        string StartDryRun(List<List<string>> csvData, bool addImportTag, string configurationSlug, string accountId);
        string StartImport(string dryRunId);
    }
}
