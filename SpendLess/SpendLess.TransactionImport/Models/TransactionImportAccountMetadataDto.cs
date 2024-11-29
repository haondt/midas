using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using SpendLess.Domain.Models;

namespace SpendLess.TransactionImport.Models
{
    public class TransactionImportAccountMetadataDto
    {
        public Optional<StorageKey<TransactionImportConfigurationDto>> DefaultConfiguration { get; set; }
    }
}
