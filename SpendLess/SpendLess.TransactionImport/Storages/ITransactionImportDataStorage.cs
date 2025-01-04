using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Storages
{
    public interface ITransactionImportDataStorage
    {
        Task Add(TransactionImportData data);
        Task AddMany(IEnumerable<TransactionImportData> datum);
        Task<List<bool>> CheckIfHasSourceDataHash(IEnumerable<long> hashes);
        Task<List<TransactionImportData>> Get(long transactionId);
        Task<List<List<TransactionImportData>>> GetMany(IEnumerable<long> transactionIds);
    }
}