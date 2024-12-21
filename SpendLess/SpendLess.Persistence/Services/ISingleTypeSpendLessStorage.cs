using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace SpendLess.Persistence.Services
{
    public interface ISingleTypeSpendLessStorage<T> where T : notnull
    {
        Task Set(StorageKey<T> primarkey, T value);
        Task Set(StorageKey<T> primarkey, T value, List<StorageKey<T>> foreignKeys);
        Task<T> Get(StorageKey<T> primaryKey);
        Task<bool> ContainsKey(StorageKey<T> primaryKey);
        Task Delete(StorageKey<T> primaryKey);
        Task<Dictionary<StorageKey<T>, T>> GetAll();
        Task<Optional<T>> TryGet(StorageKey<T> primaryKey);
        Task<bool> ContainsForeignKey(StorageKey<T> foreignKey);
        Task<List<Optional<T>>> TryGetMany(List<StorageKey<T>> primaryKeys);
    }
}
