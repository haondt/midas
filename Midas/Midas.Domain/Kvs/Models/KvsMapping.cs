namespace Midas.Domain.Kvs.Models
{
    public class KvsMapping
    {
        public required string Key { get; set; }
        public string Value { get; set; } = "";
        public required List<string> Aliases { get; set; }
    }
}
