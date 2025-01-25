namespace Midas.UI.Components.Reporting.TableFactories
{
    public class ReportingTableData
    {
        public List<string> Headers { get; set; } = [];
        public List<List<ReportingTableDataCell>> Rows { get; set; } = [];
    }
}
