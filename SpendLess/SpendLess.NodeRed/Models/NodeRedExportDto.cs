using Newtonsoft.Json;

namespace SpendLess.NodeRed.Models
{
    public class NodeRedExportDto
    {
        [JsonProperty("flows.json"), JsonRequired]
        public required string Flows { get; set; }

        [JsonProperty("settings.js"), JsonRequired]
        public required string Settings { get; set; }

    }
}
