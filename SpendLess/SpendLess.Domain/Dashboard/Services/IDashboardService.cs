using SpendLess.Domain.Dashboard.Models;

namespace SpendLess.Domain.Dashboard.Services
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GatherData(DateTime start, DateTime end);
    }
}