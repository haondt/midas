using Midas.UI.Models.Transactions;

namespace Midas.UI.Components.Reporting.TableFactories
{
    public partial class ReportingTableDataFactory
    {
        public required List<(string AccountName, decimal Amount, int TransactionsCount, decimal AverageAmountPerTransaction)> TopIncomeSources { get; set; }
        public required List<(string AccountName, decimal Amount, int TransactionsCount, decimal AverageAmountPerTransaction)> TopSpendingDestinations { get; set; }

        public ReportingTableData CreateTopIncomeSourcesTable()
        {
            return new ReportingTableData
            {
                Headers = new()
                {
                    "Source",
                    "Amount",
                    "Per transaction",
                    "Per month",
                },
                Rows = TopIncomeSources.Select(q => new List<ReportingTableDataCell>
                {

                    new ReportingTableDataTransactionFilterLinkCell
                    {
                        Text = q.AccountName,
                        Filters = new List<string>()
                        {
                            $"{TransactionFilterTargets.SourceAccountName} {TransactionFilterOperators.IsEqualTo} {q.AccountName}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsGreaterThanOrEqualTo} {_startDateString}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsLessThanOrEqualTo} {_endDateString}",

                        }
                    },
                    new ReportingTableDataAmountCell { Amount = q.Amount },
                    new ReportingTableDataAmountCell { Amount = q.AverageAmountPerTransaction },
                    new ReportingTableDataAmountCell { Amount = AmortizeAmount(q.Amount) }
                }).ToList()
            };
        }
        public ReportingTableData CreateTopSpendingDestinationsTable()
        {
            return new ReportingTableData
            {
                Headers = new()
                {
                    "Destination",
                    "Amount",
                    "Per transaction",
                    "Per month",
                },
                Rows = TopSpendingDestinations.Select(q => new List<ReportingTableDataCell>
                {

                    new ReportingTableDataTransactionFilterLinkCell
                    {
                        Text = q.AccountName,
                        Filters = new List<string>()
                        {
                            $"{TransactionFilterTargets.DestinationAccountName} {TransactionFilterOperators.IsEqualTo} {q.AccountName}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsGreaterThanOrEqualTo} {_startDateString}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsLessThanOrEqualTo} {_endDateString}",
                        }
                    },
                    new ReportingTableDataAmountCell { Amount = q.Amount },
                    new ReportingTableDataAmountCell { Amount = q.AverageAmountPerTransaction },
                    new ReportingTableDataAmountCell { Amount = AmortizeAmount(q.Amount) }
                }).ToList()
            };
        }
    }
}
