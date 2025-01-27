using Midas.Core.Models;
using Midas.Domain.Import.Models;
using Midas.Domain.Transactions.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Split.Models
{
    public class TransactionSplit
    {
        public required string Description { get; set; }
        public required string SourceAccount { get; set; }
        public required string SourceAccountName { get; set; }
        public required string DestinationAccount { get; set; }
        public required string DestinationAccountName { get; set; }
        public required decimal Amount { get; set; }
        public required AbsoluteDateTime Date { get; set; }
        public required string Category { get; set; }
        public required List<string> Tags { get; set; }
        public required List<TransactionSplitImportData> ImportDatum { get; set; }

        public ExtendedTransactionDto AsExtendedTransaction()
        {
            return new ExtendedTransactionDto
            {
                Description = Description,
                SourceAccount = SourceAccount,
                SourceAccountName = SourceAccountName,
                DestinationAccount = DestinationAccount,
                DestinationAccountName = DestinationAccountName,
                Amount = Amount,
                Category = Category,
                ImportDatum = ImportDatum.Select(q => q.AsExtendedImportData()).ToList(),
                Tags = Tags.ToHashSet(),
                TimeStamp = Date
            };
        }
        public TransactionDto AsTransaction()
        {
            return new TransactionDto
            {
                Description = Description,
                SourceAccount = SourceAccount,
                DestinationAccount = DestinationAccount,
                Amount = Amount,
                Category = Category,
                Tags = Tags.ToHashSet(),
                TimeStamp = Date
            };
        }
    }

    public class TransactionSplitImportData
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
                _sourceDataString = TransactionImportData.StringifySourceData(SourceData);
                _sourceDataHash = TransactionImportData.HashSourceData(_sourceDataString);
            }
        }

        private long _sourceDataHash = 0;
        public long SourceDataHash { get { return _sourceDataHash; } }

        private string _sourceDataString = default!;
        public string SourceDataString { get { return _sourceDataString; } }

        public TransactionImportDataDto AsImportDataDto(long transactionId)
        {
            return new()
            {
                Account = Account,
                ConfigurationSlug = ConfigurationSlug,
                SourceData = SourceData,
                Transaction = transactionId,
            };
        }
        public ExtendedTransactionImportData AsExtendedImportData()
        {
            return new()
            {
                Account = Account,
                ConfigurationSlug = ConfigurationSlug,
                SourceData = SourceData,
            };
        }
    }


}
