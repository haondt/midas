
namespace Midas.Domain.Supercategories.Services
{
    public interface ISupercategoryService
    {
        Task DeleteMapping(string category);
        Task DeleteSupercategory(string supercategory);
        Task<Dictionary<string, List<string>>> GetSupercategories();
        Task<List<string>> GetUnmappedCategories();
        Task<List<string>> SearchSupercategories(string partialName);
        Task UpsertMapping(string category, string supercategory);
    }
}
