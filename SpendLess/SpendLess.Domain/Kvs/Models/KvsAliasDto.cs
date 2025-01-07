using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Kvs.Models
{
    public class KvsAliasDto
    {
        public required StorageKey<KvsMappingDto> Key { get; set; }
    }
}
