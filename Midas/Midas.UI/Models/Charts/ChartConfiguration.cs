using Newtonsoft.Json;

namespace Midas.UI.Models.Charts
{

    public class ChartConfiguration
    {
        public required string Type { get; set; }
        public ChartData Data { get; set; } = new();
        public ChartOptions Options { get; set; } = new();
        public List<ChartDataSet>? DataSets { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, ChartConstants.SerializerSettings);
        }

    }
}
