namespace SpendLess.Domain.Dashboard.Models
{
    public class DashboardDataDto
    {
        public required decimal CashFlow { get; set; }
        public required DashboardBalanceChartDataDto BalanceChartData { get; set; }
        public required DashboardCategoricalSpendingChartDataDto CategoricalSpendingChartData { get; set; }

        public required decimal NetWorth { get; set; }
        public required decimal Income { get; set; }
        public required decimal Spending { get; set; }
    }
}
