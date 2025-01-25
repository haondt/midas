namespace Midas.UI.Components.Reporting.TableFactories
{
    public partial class ReportingTableDataFactory
    {
        public required List<(string AccountName, decimal Income, decimal Spending, decimal CashFlow)> AccountSpending { get; set; }
        public required double DurationInMonths { get; set; }

        public ReportingTableData CreateSummaryTable()
        {
            return new ReportingTableData
            {
                Headers = new()
                {
                    "Total income",
                    "Total spending",
                    "Total cash flow",
                    "Monthly cash flow"
                },
                Rows = new() { new ()
                {
                    new ReportingTableDataAmountCell { Amount = TotalIncome },
                    new ReportingTableDataAmountCell { Amount = TotalSpending },
                    new ReportingTableDataAmountCell { Amount = TotalCashFlow, IsDelta = true },
                    new ReportingTableDataAmountCell { Amount = AmortizeAmount(TotalCashFlow), IsDelta = true },
                } }
            };
        }


        public ReportingTableData CreateAccountBalanceTable()
        {
            return new ReportingTableData
            {
                Headers = new()
                {
                    "Account",
                    "Income",
                    "Spending",
                    "Cash flow",
                    "Cash flow (/mo)"
                },
                Rows = AccountSpending.Select(x => new List<ReportingTableDataCell>()
                {
                    new ReportingTableDataStringCell { Value = x.AccountName },
                    new ReportingTableDataAmountCell { Amount = x.Income },
                    new ReportingTableDataAmountCell { Amount = x.Spending },
                    new ReportingTableDataAmountCell { Amount = x.CashFlow, IsDelta = true },
                    new ReportingTableDataAmountCell
                    {
                        Amount = AmortizeAmount(x.CashFlow),
                        IsDelta = true
                    },
                }).ToList()
            };
        }

    }
}
