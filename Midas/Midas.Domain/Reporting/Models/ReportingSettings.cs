namespace Midas.Domain.Reporting.Models
{
    public class ReportingSettings
    {
        public int TopIncomeSourcesLimit { get; set; } = 5;
        public int TopSpendingDestinationsLimit { get; set; } = 5;

    }
}
