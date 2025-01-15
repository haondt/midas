using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Supercategories.Services
{
    public class SupercategoryService(ISupercategoryStorage supercategoryStorage, ITransactionStorage transactionStorage) : ISupercategoryService
    {
        public async Task<List<string>> GetUnmappedCategories()
        {
            var categories = await transactionStorage.GetCategories();
            var mappedCategories = (await supercategoryStorage.GetAllCategories()).ToHashSet();

            return categories.Where(c => !mappedCategories.Contains(c)).ToList();
        }

        public Task<Dictionary<string, List<string>>> GetSupercategories()
        {
            return supercategoryStorage.GetAllSupercategories();
        }

        public Task<List<string>> SearchSupercategories(string partialName)
        {
            return supercategoryStorage.SearchSupercategories(partialName, 5); // todo: appsettings, maybe should just have a global one for autocompletes
        }

        public Task UpsertMapping(string category, string supercategory)
        {
            return supercategoryStorage.SetSupercategory(category, supercategory);
        }

        public Task DeleteSupercategory(string supercategory)
        {
            return supercategoryStorage.DeleteSupercategory(supercategory);
        }

        public Task DeleteMapping(string category)
        {
            return supercategoryStorage.UnsetSupercategory(category);
        }
    }
}
