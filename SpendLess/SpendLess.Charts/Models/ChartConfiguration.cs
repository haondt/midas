using Newtonsoft.Json;
using SpendLess.Charts.Constants;

namespace SpendLess.Charts.Models
{

    public abstract class ChartConfiguration
    {
        public abstract string Type { get; }
        public ChartData Data { get; set; } = new();
        public ChartOptions Options { get; set; } = new();
        public List<ChartDataSet>? DataSets { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, ChartConstants.SerializerSettings);
        }

    }
}
