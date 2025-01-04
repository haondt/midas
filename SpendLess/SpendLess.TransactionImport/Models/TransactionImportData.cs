using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using System.Text;

namespace SpendLess.TransactionImport.Models
{
    public class TransactionImportData
    {
        public required long Transaction { get; set; }
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
            return JsonConvert.SerializeObject(sourceData, SpendLessConstants.ApiSerializerSettings);
        }

        public static List<string> DestringifySourceData(string sourceData)
        {
            return JsonConvert.DeserializeObject<List<string>>(sourceData, SpendLessConstants.ApiSerializerSettings) ?? throw new InvalidOperationException($"Unable to destringify source data {sourceData}");
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
