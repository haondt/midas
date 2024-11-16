using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Models
{
    public class KvsAliasDto
    {
        public required StorageKey<KvsMappingDto> Key { get; set; }
    }
}
