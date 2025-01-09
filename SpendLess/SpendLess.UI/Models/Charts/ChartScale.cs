namespace SpendLess.UI.Models.Charts
{
    public class ChartScale
    {
        public bool? BeginAtZero { get; set; }
        public bool? Stacked { get; set; }
        public string? Position { get; set; }
        public ChartTitle? Title { get; set; }
        public ChartTicks? Ticks { get; set; }
        public ChartGrid? Grid { get; set; }
    }
}