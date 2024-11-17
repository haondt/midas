using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Models;

namespace SpendLess.NodeRed.Models
{
    public class SendToNodeRedRequestDto
    {
        public string Account { get; set; } = "";
        public TransactionImportConfigurationDto Configuration { get; set; } = new();
        public CsvData? Csv { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, SpendLessConstants.ApiSerializerSettings);
        }
    }

    public class CsvData
    {
        public required List<string> Row { get; set; }
        public required List<string> FirstRow { get; set; }
    }
}
