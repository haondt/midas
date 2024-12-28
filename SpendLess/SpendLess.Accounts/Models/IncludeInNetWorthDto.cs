using Haondt.Identity.StorageKey;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace SpendLess.Accounts.Models
{
    public class IncludeInNetWorthDto
    {
        public static StorageKey<AccountDto> GetForeignKey() => true.SeedStorageKey<IncludeInNetWorthDto>().Extend<AccountDto>();
        public static bool TryParseForeignKey(StorageKey<AccountDto> storageKey, [MaybeNullWhen(false)] out bool? value)
        {
            value = null;
            var first = storageKey.First();
            if (first.Type != typeof(IncludeInNetWorthDto))
                return false;
            if (!bool.TryParse(first.Value, out var parsed))
                return false;
            value = parsed;
            return true;
        }
    }
}
