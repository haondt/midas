using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Models;
using Midas.Domain.Reporting.Services;
using Midas.UI.Components.Reporting;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Reporting
{
    [Route("report")]
    public class ReportController(IComponentFactory componentFactory, IReportService reportService) : MidasUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get(
            [FromQuery(Name = "f")] AbsoluteDateTime from,
            [FromQuery(Name = "t")] AbsoluteDateTime to)
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
                SupercategoricalSpending = report.SupercategoricalSpending,
                TopIncomeSources = report.TopIncomeSources,
                TopSpendingDestinations = report.TopSpendingDestinations,
                CategoricalSpendingPerTimeStep = report.CategoricalSpendingPerTimeStep
            });
        }
    }
}
