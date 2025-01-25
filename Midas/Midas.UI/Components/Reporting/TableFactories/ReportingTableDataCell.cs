namespace Midas.UI.Components.Reporting.TableFactories
{
    public abstract class ReportingTableDataCell
    {
    }

    public class ReportingTableDataStringCell : ReportingTableDataCell
    {
        public required string Value { get; set; }
    }
    public class ReportingTableDataAmountCell : ReportingTableDataCell
    {
        public required decimal Amount { get; set; }
        public bool IsDelta { get; set; } = false;
    }
    public class ReportingTableDataTransactionFilterLinkCell : ReportingTableDataCell
    {
        public required string Text { get; set; }
        public required List<string> Filters { get; set; }
    }
}
