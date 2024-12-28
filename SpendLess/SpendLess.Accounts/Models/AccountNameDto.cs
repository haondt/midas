using Haondt.Identity.StorageKey;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace SpendLess.Accounts.Models
{
    public class AccountNameDto
    {
        public static StorageKey<AccountDto> GetForeignKey(string name) => name.SeedStorageKey<AccountNameDto>().Extend<AccountDto>();
        public static bool TryParseForeignKey(StorageKey<AccountDto> storageKey, [MaybeNullWhen(false)] out string? value)
        {
            value = null;
            var first = storageKey.First();
            if (first.Type != typeof(AccountNameDto))
                return false;
            value = first.Value;
            return true;
        }
    }
}
