using Midas.Persistence.Models;

namespace Midas.Persistence.Storages.Abstractions
{
    public interface ITransactionImportDataStorage
    {
        Task Add(TransactionImportDataDto data);
        Task AddMany(IEnumerable<TransactionImportDataDto> datum);
        Task<List<bool>> CheckIfHasSourceDataHash(IEnumerable<long> hashes);
        Task<List<TransactionImportDataDto>> Get(long transactionId);
        Task<List<List<TransactionImportDataDto>>> GetMany(IEnumerable<long> transactionIds);
    }
}