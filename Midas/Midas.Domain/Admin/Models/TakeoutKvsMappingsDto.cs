namespace Midas.Domain.Admin.Models
{
    public class TakeoutKvsMappingsDto
    {
        public int Version { get; set; } = 0;
        public Dictionary<string, TakeoutKvsMappingDto> Mappings { get; set; } = [];
    }

    public class TakeoutKvsMappingDto
    {
        public List<string>? Aiases { get; set; }
        public string? Value { get; set; }
    }
}
