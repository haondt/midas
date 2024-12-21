using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using System.Text;

namespace SpendLess.Domain.Models
{
    public class TransactionDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = SpendLessConstants.DefaultCategory;
        public string SourceAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public string DestinationAccount { get; set; } = SpendLessConstants.DefaultAccount;
        public string Description { get; set; } = "";
        public required long TimeStamp { get; set; }
        public required string ImportAccount { get; set; }

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
                _sourceDataString = StringifySourceData(SourceData);
                _sourceDataHash = HashSourceData(_sourceDataString);
            }
        }

        private long _sourceDataHash = 0;
        public long SourceDataHash { get { return _sourceDataHash; } }

        private string _sourceDataString = default!;
        public string SourceDataString { get { return _sourceDataString; } }

        public static string StringifySourceData(List<string> sourceData)
        {
            return JsonConvert.SerializeObject(sourceData);
        }

        public static List<string> DestringifySourceData(string sourceData)
        {
            return JsonConvert.DeserializeObject<List<string>>(sourceData) ?? throw new InvalidOperationException($"Unable to destringify source data {sourceData}");
        }

        public static long HashSourceData(string sourceDataString)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sourceDataString));
            return BitConverter.ToInt64(bytes, 0);
        }

        public static long HashSourceData(List<string> sourceData)
        {
            return HashSourceData(StringifySourceData(sourceData));
        }

    }
}
