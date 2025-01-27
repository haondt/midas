using Midas.Domain.Split.Models;
using Midas.Domain.Transactions.Models;

namespace Midas.Domain.Split.Services
{
    public interface ISplitService
    {
        Task<TransactionSplitDryRunResultDto> DryRun(TransactionSplitOptions splitOptions);
        Task<List<(long Id, ExtendedTransactionDto Transaction)>> PerformSplit(TransactionSplitOptions splitOptions);
    }
}
