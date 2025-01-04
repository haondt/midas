using Haondt.Core.Models;
using SpendLess.Domain.Constants;
using SpendLess.TransactionImport.Models;

namespace SpendLess.Domain.Models
{
    public class DryRunResultDto
    {
        public List<DryRunTransactionDto> Transactions { get; set; } = [];
        public Dictionary<string, string> NewAccounts { get; set; } = [];
        public Dictionary<string, int> NewCategories { get; set; } = [];
        public Dictionary<string, int> NewTags { get; set; } = [];
        public decimal BalanceChange { get; set; } = 0;
        public Optional<string> ImportTag { get; set; }
    }

    public class DryRunTransactionDto
    {
        public required string SourceRequestPayload { get; set; }
        public required DryRunTransactionImportData ImportData { get; set; }
        public required List<string> SourceData { get; set; }
        public HashSet<string> Warnings { get; set; } = [];
        public HashSet<string> Errors { get; set; } = [];
        public string Status => Errors.Count > 0 ? TransactionImportStatus.Error
            : Warnings.Count > 0 ? TransactionImportStatus.Warning
            : TransactionImportStatus.Success;
        public Optional<DryRunTransactionDataDto> TransactionData { get; set; } = new();
        public Optional<long> ReplacementTarget { get; set; }
    }

    public class DryRunTransactionDataDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
        public required long TimeStamp { get; set; }
        public required DryRunAccountDto Source { get; set; }
        public required DryRunAccountDto Destination { get; set; }
        public string Description { get; set; } = "";
    }

    public class DryRunTransactionImportData
    {
        public required string Account { get; set; }
        public required string ConfigurationSlug { get; set; }
        private List<string> _sourceData = default!;
        public required List<string> SourceData
        {
            get
            {
                return _sourceData;
            }
            set
            {
                _sourceData = value;
                _sourceDataHash = TransactionImportData.HashSourceData(_sourceData);
            }
        }

        private long _sourceDataHash = 0;
        public long SourceDataHash { get { return _sourceDataHash; } }

    }

    public class DryRunAccountDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}
