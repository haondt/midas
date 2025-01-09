namespace SpendLess.Persistence.Storages.Abstractions
{
    public interface IKvsStorage
    {
        Task AddKey(string key);
        Task RemoveKey(string key);
        Task<List<string>> GetAllKeys();
        Task<List<string>> SearchKey(string partialKey);
    }
}
