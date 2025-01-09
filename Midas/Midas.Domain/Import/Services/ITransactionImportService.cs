using Haondt.Core.Models;
using Midas.Domain.Import.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Import.Services
{
    public interface ITransactionImportService
    {
        Result<DryRunResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId);
        Result<TransactionImportResultDto, (double Progress, Optional<string> ProgressMessage)> GetImportResult(string jobId);
        string StartDryRun(List<TransactionFilter> filters, bool addImportTag, string? configurationSlug = null, string? accountId = null);
        string StartDryRun(List<List<string>> csvData, bool addImportTag, string configurationSlug, string accountId, TransactionImportConflictResolutionStrategy conflictResolutionStrategy);
        string StartImport(string dryRunId);
    }
}
