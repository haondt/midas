using Haondt.Core.Models;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Exceptions;
using Midas.Domain.Dashboard.Models;
using Midas.Domain.Dashboard.Services;
using Midas.UI.Components.Dashboard;
using Midas.UI.Shared.Controllers;

namespace Midas.Dashboard.Controllers
{
    [Route("dashboard")]
    public class DashboardController(IComponentFactory componentFactory, IDashboardService dashboardService) : MidasUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get()
        {
            var currentTime = AbsoluteDateTime.Now;
            var currentMonth = currentTime.FloorToLocalMonth();
            var endOfMonth = currentMonth.AddLocalMonths(1);
            var data = await GetDashboardData(currentMonth, endOfMonth);
            return await componentFactory.RenderComponentAsync<Midas.UI.Components.Dashboard.Dashboard>(new()
            {
                DashboardData = data
            });
        }

        private async Task<DashboardData> GetDashboardData(AbsoluteDateTime start, AbsoluteDateTime end)
        {
            var dto = await dashboardService.GatherData(start, end);
            return new()
            {
                CashFlow = dto.CashFlow,
                BalanceChartData = dto.BalanceChartData,
                CategoricalSpendingChartData = dto.CategoricalSpendingChartData,
                Income = dto.Income,
                NetWorth = dto.NetWorth,
                Spending = dto.Spending,
            };
        }

        [HttpGet("data")]
        public async Task<IResult> GetData(
            [FromQuery] string? r,
            [FromQuery] AbsoluteDateTime? f,
            [FromQuery] AbsoluteDateTime? t)
        {
            AbsoluteDateTime? start = null;
            AbsoluteDateTime? end = null;
            if (long.TryParse(r, out var ts))
            {
                start = AbsoluteDateTime.Create(ts);
                end = start.Value.AddMonths(1);
            }
            else if (r == DashboardRange.YearToDate)
            {
                end = AbsoluteDateTime.Now;
                start = AbsoluteDateTime.Now.FloorToLocalYear();
            }
            else if (r == DashboardRange.Everything)
            {
                end = AbsoluteDateTime.MaxValue;
                start = AbsoluteDateTime.MinValue;
            }
            else if (f.HasValue && t.HasValue)
            {
                if (t.Value < f.Value)
                    throw new UserException("\"To\" date must come after \"From\" date.");
                start = f;
                end = t;
            }
            else
            {
                throw new UserException("Unable to parse range query");
            }

            var data = await GetDashboardData(start.Value, end.Value);
            return await componentFactory.RenderComponentAsync<DashboardData>(data);
        }

    }
}
