using Haondt.Core.Models;
using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Models
{
    public class AccountDto
    {
        public required string Name { get; set; }
        public required decimal Balance { get; set; }
        public bool IsWatched { get; set; } = false;
        public Optional<StorageKey<TransactionImportConfigurationDto>> DefaultImportConfiguration { get; set; } = new();
    }
}
