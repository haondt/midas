using Midas.Core.Constants;
using Midas.Core.Models;

namespace Midas.Domain.Admin.Models
{
    public class TakeoutTransactionsDto
    {
        public int Version { get; set; } = 0;
        public List<TakeoutTransactionDto> Transactions { get; set; } = [];

    }
    public class TakeoutTransactionDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = MidasConstants.DefaultCategory;
        public string SourceAccount { get; set; } = MidasConstants.DefaultAccount;
        public string DestinationAccount { get; set; } = MidasConstants.DefaultAccount;
        public string Description { get; set; } = "";
        public required AbsoluteDateTime TimeStamp { get; set; }

        public List<TakeoutTransactionImportDataDto> ImportData { get; set; } = [];
    }

    public class TakeoutTransactionImportDataDto
    {
        public required string Account { get; set; }
        public required string ConfigurationSlug { get; set; }
        public required List<string> SourceData { get; set; }
    }
}
