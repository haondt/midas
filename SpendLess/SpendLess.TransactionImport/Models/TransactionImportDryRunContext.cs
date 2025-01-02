using SpendLess.Domain.Models;

namespace SpendLess.TransactionImport.Models
{
    public class TransactionImportDryRunContext
    {
        public required DryRunResultDto Result { get; set; }
        public long CurrentTimeStamp { get; set; }
        public required HashSet<string> ExistingCategories { get; set; }
        public required HashSet<string> ExistingTags { get; set; }
    }
}
