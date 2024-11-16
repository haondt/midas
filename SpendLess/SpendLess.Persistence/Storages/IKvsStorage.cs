

namespace SpendLess.Persistence.Storages
{
    public interface IKvsStorage
    {
        Task AddKey(string key);
        Task RemoveKey(string key);
        Task<List<string>> SearchKey(string partialKey);
    }
}
