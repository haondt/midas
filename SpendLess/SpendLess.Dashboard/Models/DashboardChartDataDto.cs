namespace SpendLess.Dashboard.Models
{
    public class DashboardChartDataDto
    {
        public required List<string> AccountNames { get; set; }
        public required List<DateTime> TimeStamps { get; set; }
        public required List<List<decimal>> Balances { get; set; }
    }
}
