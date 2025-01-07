using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Domain.Reporting.Services;
using SpendLess.UI.Components.Reporting;
using SpendLess.UI.Shared.Controllers;

namespace SpendLess.UI.Controllers.Reporting
{
    [Route("report")]
    public class ReportController(IComponentFactory componentFactory, IReportService reportService) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get(
            [FromQuery(Name = "f")] DateTime from,
            [FromQuery(Name = "t")] DateTime to)
        {

            var report = await reportService.GenerateReportData(from, to);
            return await componentFactory.RenderComponentAsync(new Report
            {
                StartTime = report.StartTime,
                EndTime = report.EndTime,
                CashFlowPerTimeStep = report.CashFlowPerTimeStep,
                IncomePerTimeStep = report.IncomePerTimeStep,
                SpendingPerTimeStep = report.SpendingPerTimeStep,
                TimeStepLabels = report.TimeStepLabels,
                TotalSpending = report.TotalSpending,
                AccountSpending = report.AccountSpending,
                CategoricalSpending = report.CategoricalSpending,
                TopIncomeSources = report.TopIncomeSources,
                TopSpendingDestinations = report.TopSpendingDestinations,
                CategoricalSpendingPerTimeStep = report.CategoricalSpendingPerTimeStep
            });
        }
    }
}
