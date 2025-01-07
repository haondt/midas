using SpendLess.Domain.Reporting.Models;

namespace SpendLess.Domain.Reporting.Services
{
    public interface IReportService
    {
        Task<ReportDataDto> GenerateReportData(DateTime start, DateTime end);
    }
}