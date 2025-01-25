using Midas.UI.Models.Transactions;

namespace Midas.UI.Components.Reporting.TableFactories
{
    public partial class ReportingTableDataFactory
    {
        public required List<(string CategoryName, decimal Amount)> CategoricalSpending { get; set; }

        public ReportingTableData CreateCategoricalSpendingTable()
        {
            return new ReportingTableData
            {
                Headers = new()
                {
                    "Category",
                    "Amount",
                    "Amount (/mo)",
                },
                Rows = CategoricalSpending.Select(q => new List<ReportingTableDataCell>
                {

                    new ReportingTableDataTransactionFilterLinkCell
                    {
                        Text = q.CategoryName,
                        Filters = new List<string>()
                        {
                            $"{TransactionFilterTargets.Category} {TransactionFilterOperators.IsEqualTo} {q.CategoryName}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsGreaterThanOrEqualTo} {_startDateString}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsLessThanOrEqualTo} {_endDateString}",
                        }
                    },
                    new ReportingTableDataAmountCell { Amount = q.Amount },
                    new ReportingTableDataAmountCell { Amount = AmortizeAmount(q.Amount) },
                }).ToList()
            };
        }
    }
}
