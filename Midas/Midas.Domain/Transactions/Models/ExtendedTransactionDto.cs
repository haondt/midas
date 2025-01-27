using Midas.Domain.Import.Models;
using Midas.Persistence.Models;

namespace Midas.Domain.Transactions.Models
{
    public class ExtendedTransactionDto : TransactionDto
    {
        public required string SourceAccountName { get; set; }
        public required string DestinationAccountName { get; set; }
        public required List<ExtendedTransactionImportData> ImportDatum { get; set; }

    }

    public class ExtendedTransactionImportData
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


        public static implicit operator ExtendedTransactionImportData(TransactionImportDataDto data) => new()
        {
            Account = data.Account,
            ConfigurationSlug = data.ConfigurationSlug,
            SourceData = data.SourceData,
        };
    }
}
