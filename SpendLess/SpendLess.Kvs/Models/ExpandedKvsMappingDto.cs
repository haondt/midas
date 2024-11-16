using SpendLess.Persistence.Models;

namespace SpendLess.Kvs.Models
{
    public class ExpandedKvsMappingDto : KvsMappingDto
    {
        public required string Key { get; set; }
        public required List<string> Aliases { get; set; }
    }
}
