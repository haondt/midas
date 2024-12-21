namespace SpendLess.Charts.Models
{
    public class ChartDataSet
    {
        public string? Label { get; set; }
        public List<double> Data { get; set; } = [];
        public double? BorderWidth { get; set; }
    }
}