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
                Rows = SupercategoricalSpending.Select(q => new List<ReportingTableDataCell>
                {

                    new ReportingTableDataStringCell { Value = q.SupercategoryName },
                    new ReportingTableDataAmountCell { Amount = q.Amount },
                    new ReportingTableDataAmountCell { Amount = AmortizeAmount(q.Amount) },
                }).ToList()
            };
        }
    }
}
