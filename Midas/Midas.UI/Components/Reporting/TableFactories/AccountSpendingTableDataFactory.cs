using Midas.UI.Models.Transactions;

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
                Rows = AccountSpending.Select(q => new List<ReportingTableDataCell>()
                {
                    new ReportingTableDataTransactionFilterLinkCell
                    {
                        Text = q.AccountName,
                        Filters = new List<string>()
                        {
                            $"{TransactionFilterTargets.EitherAccountName} {TransactionFilterOperators.IsEqualTo} {q.AccountName}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsGreaterThanOrEqualTo} {_startDateString}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsLessThanOrEqualTo} {_endDateString}",
                        }
                    },
                    new ReportingTableDataAmountCell { Amount = q.Income },
                    new ReportingTableDataAmountCell { Amount = q.Spending },
                    new ReportingTableDataAmountCell { Amount = q.CashFlow, IsDelta = true },
                    new ReportingTableDataAmountCell
                    {
                        Amount = AmortizeAmount(q.CashFlow),
                        IsDelta = true
                    },
                }).ToList()
            };
        }

    }
}
