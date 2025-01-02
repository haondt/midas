using Haondt.Core.Models;
using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Storages
{
    public interface ITransactionImportDataStorage
    {
        Task<Optional<TransactionImportData>> Get(long transactionId);
        Task<Dictionary<long, TransactionImportData>> GetMany(List<long> transactionIds);
        Task Set(long transactionId, TransactionImportData data);
        Task SetMany(Dictionary<long, TransactionImportData> datum);
    }
}