using Midas.Domain.Merge.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Merge.Services
{
    public interface IMergeService
    {
        Task<TransactionMergeDefaultValuesDto> CreateDefaults(List<TransactionFilter> filters);
        Task<TransactionMergeDryRunResultDto> DryRun(TransactionMergeDryRunOptions options);
        Task<long> PerformMerge(TransactionMergeDryRunOptions options);
    }

}
