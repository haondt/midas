using Haondt.Core.Models;

namespace Midas.Persistence.Storages.Abstractions
{
    public interface ISupercategoryStorage
    {
        Task AddManySupercategories(IEnumerable<(string category, string supercategory)> items);
        Task DeleteAllSupercategories();
        Task DeleteSupercategory(string supercategory);
        Task<List<string>> GetAllCategories();
        Task<Dictionary<string, List<string>>> GetAllSupercategories();
        Task<List<string>> GetCategories(string supercategory);
        Task<Optional<string>> GetSupercategory(string category);
        Task<List<string>> SearchSupercategories(string partialName, int limit);
        Task SetManySupercategories(IEnumerable<(string category, string supercategory)> items);
        Task SetSupercategory(string category, string supercategory);
        Task UnsetManySupercategories(List<string> categories);
        Task UnsetSupercategory(string category);
    }
}