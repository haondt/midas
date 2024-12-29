
namespace SpendLess.Reporting.Services
{
    public interface IReportService
    {
        Task<ReportDataDto> GenerateReportData(DateTime start, DateTime end);
    }
}