namespace SpendLess.Dashboard.Models
{
    public class DashboardDataDto
    {
        public required decimal CashFlow { get; set; }
        public required DashboardBalanceChartDataDto BalanceChartData { get; set; }
        public required DashboardCategoricalSpendingChartDataDto CategoricalSpendingChartData { get; set; }
    }
}
