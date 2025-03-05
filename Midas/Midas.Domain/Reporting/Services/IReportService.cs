using Haondt.Core.Models;
using Midas.Domain.Reporting.Models;

namespace Midas.Domain.Reporting.Services
{
    public interface IReportService
    {
        Task<ReportDataDto> GenerateReportData(AbsoluteDateTime start, AbsoluteDateTime end);
    }
}