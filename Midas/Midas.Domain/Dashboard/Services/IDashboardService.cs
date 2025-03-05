using Haondt.Core.Models;
using Midas.Domain.Dashboard.Models;

namespace Midas.Domain.Dashboard.Services
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GatherData(AbsoluteDateTime start, AbsoluteDateTime end);
    }
}