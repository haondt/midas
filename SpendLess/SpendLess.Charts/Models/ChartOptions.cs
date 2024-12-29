namespace SpendLess.Charts.Models
{
    public class ChartOptions
    {
        public ChartScales? Scales { get; set; }
        public ChartPlugins? Plugins { get; set; }
        public bool Responsive { get; set; } = true;
        public bool MaintainAspectRatio { get; set; } = false;
    }
}

