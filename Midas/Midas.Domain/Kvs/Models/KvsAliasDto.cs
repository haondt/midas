using Haondt.Identity.StorageKey;

namespace Midas.Domain.Kvs.Models
{
    public class KvsAliasDto
    {
        public required StorageKey<KvsMappingDto> Key { get; set; }
    }
}
