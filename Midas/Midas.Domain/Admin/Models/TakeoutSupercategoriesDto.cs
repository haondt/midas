namespace Midas.Domain.Admin.Models
{
    public class TakeoutSupercategoriesDto
    {
        public int Version { get; set; } = 0;
        public Dictionary<string, List<string>> Supercategories { get; set; } = [];
    }
}
