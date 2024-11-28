using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace SpendLess.Persistence.Services
{
    public interface ISingleTypeSpendLessStorage<T> where T : notnull
    {
        Task Set(StorageKey<T> primarkey, T value);
        Task<T> Get(StorageKey<T> primaryKey);
        Task Delete(StorageKey<T> primaryKey);
        Task<Dictionary<StorageKey<T>, T>> GetAll();
        Task<Optional<T>> TryGet(StorageKey<T> primaryKey);
    }
}
