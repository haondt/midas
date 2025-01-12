namespace Midas.Domain.Admin.Models
{
    public class TakeoutImportConfigurationsDto
    {
        public int Version { get; set; } = 0;
        public List<string> Slugs { get; set; } = [];
    }
}
