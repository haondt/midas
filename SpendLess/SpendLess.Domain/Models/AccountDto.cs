using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Models
{
    public class AccountDto
    {
        public static StorageKey<AccountDto> GetStorageKey(Guid id) => StorageKey<AccountDto>.Create(id.ToString());
        public required string Name { get; set; }
        public required decimal Balance { get; set; }
    }
}
