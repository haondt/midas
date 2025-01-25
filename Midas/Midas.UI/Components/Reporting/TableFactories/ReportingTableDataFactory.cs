namespace Midas.UI.Components.Reporting.TableFactories
{
    public partial class ReportingTableDataFactory
    {
        public required decimal TotalSpending { get; set; }
        public required decimal TotalIncome { get; set; }
        public required decimal TotalCashFlow { get; set; }
        private decimal AmortizeAmount(decimal amount)
        {
            return Math.Round(amount / (decimal)DurationInMonths, 2);
        }
    }
}
