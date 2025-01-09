using Midas.Core.Models;

namespace Midas.UI.Models.Charts
{
    public class ChartDataSet
    {
        public string? Label { get; set; }
        public string? Type { get; set; }
        public List<object> Data { get; set; } = [];
        public double? BorderWidth { get; set; }
        public Union<string, List<string>>? BorderColor { get; set; }
        public Union<string, List<string>>? PointBackgroundColor { get; set; }
        public Union<string, List<string>>? PointBorderColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? Color { get; set; }
        public ChartSegmentOptions? Segment { get; set; }
        public double? Tension { get; set; }
        public bool? Fill { get; set; }
        public string? YAxisID { get; set; }
    }
}