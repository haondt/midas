namespace Midas.Domain.Dashboard.Models
{
    public class DashboardBalanceChartDataDto
    {
        public required List<string> AccountNames { get; set; }
        public required List<string> TimeStepLabels { get; set; }
        public required List<List<decimal>> Balances { get; set; }
    }
}
