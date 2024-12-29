namespace SpendLess.Charts.Models
{
    public class ChartDataSet
    {
        public string? Label { get; set; }
        public List<object> Data { get; set; } = [];
        public double? BorderWidth { get; set; }
        public string? BorderColor { get; set; }
        public double? Tension { get; set; }
        public bool? Fill { get; set; }
    }
}