using SpendLess.Core.Models;
using SpendLess.Domain.Dashboard.Models;

namespace SpendLess.Domain.Dashboard.Services
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GatherData(AbsoluteDateTime start, AbsoluteDateTime end);
    }
}