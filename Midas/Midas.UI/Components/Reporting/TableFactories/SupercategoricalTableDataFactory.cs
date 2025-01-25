using Midas.Core.Constants;
using Midas.UI.Models.Transactions;

namespace Midas.UI.Components.Reporting.TableFactories
{
    public partial class ReportingTableDataFactory
    {
        public required List<(string SupercategoryName, decimal Amount)> SupercategoricalSpending { get; set; }

        public ReportingTableData CreateSupercategoricalSpendingTable()
        {
            return new ReportingTableData
            {
                Headers = new()
                {
                    "Supercategory",
                    "Amount",
                    "Amount (/mo)",
                },
                Rows = SupercategoricalSpending.Select(q =>
                {
                    var superCategoryFilters = new List<string>
                        {
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsGreaterThanOrEqualTo} {_startDateString}",
                            $"{TransactionFilterTargets.Date} {TransactionFilterOperators.IsLessThanOrEqualTo} {_endDateString}",
                        };

                    if (q.SupercategoryName == MidasConstants.DefaultSupercategory)
                        superCategoryFilters.Add($"{TransactionFilterTargets.Supercategory} {TransactionFilterOperators.IsNoneOrEqualTo} {q.SupercategoryName}");
                    else
                        superCategoryFilters.Add($"{TransactionFilterTargets.Supercategory} {TransactionFilterOperators.IsEqualTo} {q.SupercategoryName}");

                    return new List<ReportingTableDataCell>
                    {

                        new ReportingTableDataTransactionFilterLinkCell
                        {
                            Text = q.SupercategoryName,
                            Filters = superCategoryFilters
                        },
                        new ReportingTableDataAmountCell { Amount = q.Amount },
                        new ReportingTableDataAmountCell { Amount = AmortizeAmount(q.Amount) },
                    };
                }).ToList()
            };
        }
    }
}
