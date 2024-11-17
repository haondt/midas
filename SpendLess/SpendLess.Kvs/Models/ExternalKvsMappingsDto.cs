namespace SpendLess.Kvs.Models
{
    public class ExternalKvsMappingsDto
    {
        public Dictionary<string, ExternalKvsMappingDto> Mappings { get; set; } = [];
    }

    public class ExternalKvsMappingDto
    {
        public List<string>? Aliases { get; set; }
        public string? Value { get; set; }
    }
}
