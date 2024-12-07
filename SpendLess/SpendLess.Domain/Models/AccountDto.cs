using Haondt.Identity.StorageKey;
using SpendLess.Persistence.Extensions;

namespace SpendLess.Domain.Models
{
    public class AccountDto
    {
        public required string Name { get; set; }
        public required decimal Balance { get; set; }
        public bool IsWatched { get; set; } = false;

        public static StorageKey<AccountDto> GetNameForeignKey(string name) => name.SeedStorageKey<AccountName>().Extend<AccountDto>();
    }

    public class AccountName { }
}
