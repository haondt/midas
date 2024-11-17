namespace SpendLess.Kvs.Models
{
    public class ExternalKvsMappingsDto
    {
        public Dictionary<string, string> Mappings { get; set; } = [];
        public Dictionary<string, string> Aliases { get; set; } = [];
    }
}
