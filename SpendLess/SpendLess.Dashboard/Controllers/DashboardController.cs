using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Core.Exceptions;
using SpendLess.Dashboard.Components;
using SpendLess.Dashboard.Models;
using SpendLess.Dashboard.Services;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Dashboard.Controllers
{
    [Route("dashboard")]
    public class DashboardController(IComponentFactory componentFactory, IDashboardService dashboardService) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get()
        {
            var localTime = DateTime.Now;
            var currentMonth = new DateTime(localTime.Year, localTime.Month, 1);
            var endOfMonth = currentMonth.AddMonths(1);
            var data = await GetDashboardData(currentMonth, endOfMonth);
            return await componentFactory.RenderComponentAsync<Dashboard.Components.Dashboard>(new Dashboard.Components.Dashboard
            {
                NetWorth = 1_000_000,
                DashboardData = data
            });
        }

        private async Task<DashboardData> GetDashboardData(DateTime start, DateTime end)
        {
            var dto = await dashboardService.GatherData(start, end);
            return new()
            {
                CashFlow = dto.CashFlow,
                BalanceChartData = dto.BalanceChartData,
                CategoricalSpendingChartData = dto.CategoricalSpendingChartData
            };
        }

        [HttpGet("data")]
        public async Task<IResult> GetData(
            [FromQuery] string? r,
            [FromQuery] DateTime? f,
            [FromQuery] DateTime? t)
        {
            DateTime? start = null;
            DateTime? end = null;
            if (long.TryParse(r, out var ts))
            {
                start = DateTimeOffset.FromUnixTimeSeconds(ts).DateTime.ToLocalTime();
                end = start.Value.AddMonths(1);
            }
            else if (r == DashboardRange.YearToDate)
            {
                end = DateTime.Now;
                start = new DateTime(end.Value.Year, 1, 1);
            }
            else if (r == DashboardRange.Everything)
            {
                end = DateTimeOffset.MaxValue.DateTime.ToLocalTime();
                start = DateTimeOffset.MinValue.DateTime.ToLocalTime();
            }
            else if (f.HasValue && t.HasValue)
            {
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
