
namespace Midas.Persistence.Storages.Abstractions
{
    public interface ITransactionImportConfigurationStorage
    {
        Task Add(string slug);
        Task AddMany(List<string> slugs);
        Task<List<string>> GetAll();
        Task Synchronize(List<string> slugs);
    }
}
