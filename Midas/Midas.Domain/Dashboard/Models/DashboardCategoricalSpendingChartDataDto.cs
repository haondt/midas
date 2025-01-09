namespace Midas.Domain.Dashboard.Models
{
    public class DashboardCategoricalSpendingChartDataDto
    {
        public List<string> Categories { get; set; } = [];
        public List<decimal> Spending { get; set; } = [];
    }
}
