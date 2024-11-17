using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Models;

namespace SpendLess.NodeRed.Models
{
    public class SendToNodeRedRequestDto
    {
        public string Account { get; set; } = "";
        public TransactionImportConfigurationDto Configuration { get; set; } = new();
        public List<string> Data { get; set; } = [];

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, SpendLessConstants.ApiSerializerSettings);
        }
    }
}
