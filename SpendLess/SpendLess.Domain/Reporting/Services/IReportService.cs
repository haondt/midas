using SpendLess.Core.Models;
using SpendLess.Domain.Reporting.Models;

namespace SpendLess.Domain.Reporting.Services
{
    public interface IReportService
    {
        Task<ReportDataDto> GenerateReportData(AbsoluteDateTime start, AbsoluteDateTime end);
    }
}