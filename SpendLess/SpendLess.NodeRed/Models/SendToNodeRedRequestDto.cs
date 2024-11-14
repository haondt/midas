using Newtonsoft.Json;
using SpendLess.Domain.Models;
using SpendLess.NodeRed.Services;

namespace SpendLess.NodeRed.Models
{
    public class SendToNodeRedRequestDto
    {
        public string AccountId { get; set; } = "";
        public TransactionImportConfigurationDto Configuration { get; set; } = new();
        public CsvData? Csv { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, NodeRedService.SerializerSettings);
        }
    }

    public class CsvData
    {
        public required List<string> Row { get; set; }
        public required List<string> FirstRow { get; set; }
    }
}
