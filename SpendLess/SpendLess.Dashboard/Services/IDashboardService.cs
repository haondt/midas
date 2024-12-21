using SpendLess.Dashboard.Models;

namespace SpendLess.Dashboard.Services
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GatherData(DateTime start, DateTime end);
    }
}