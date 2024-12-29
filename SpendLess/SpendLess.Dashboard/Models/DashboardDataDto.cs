namespace SpendLess.Dashboard.Models
{
    public class DashboardDataDto
    {
        public required decimal CashFlow { get; set; }
        public required DashboardChartDataDto ChartData { get; set; }
    }
}
