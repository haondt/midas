using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Reporting.Components;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Reporting.Controllers
{
    [Route("report")]
    public class ReportController(IComponentFactory componentFactory) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get(
            [FromQuery(Name = "f")] DateTime from,
            [FromQuery(Name = "t")] DateTime to)
        {
            return componentFactory.RenderComponentAsync(new Report
            {
                StartTime = from,
                EndTime = to,
                CashFlowPerTimeStep = new List<decimal> { 100, -100, 50, 100 },
                IncomePerTimeStep = new List<decimal> { 200, 250, 200, 150 },
                SpendingPerTimeStep = new List<decimal> { 100, 350, 150, 50 },
                TimeStepLabels = new List<string> { "January", "February", "March", "April" },
                TotalSpending = (1234, 23, -100),
                AccountSpending = new()
                {
                    ("CIBC foobar", 55, 2233, -100),
                    ("Scotiabank something", 5005, 22, 9999)
                },
                CategoricalSpending = new()
                {
                    ("Cheeses", 1000),
                    ("Bills", 2496),
                    ("Entertainment", 38),
                    ("Loans", 94.98m),
                    ("Groceries", 354.21m),
                },
                TopIncomeSources = new()
                {
                    ("Me job", 32984.23m, 8, 4123.98m),
                    ("joe shmoe", 1029, 1, 1029),
                    ("walmart refund", 32.56m, 2, 15.44m),
                },
                TopSpendingDestinations = new()
                {
                    ("shoe store", 1023.22m, 3, 333.33m),
                    ("grocery store", 2134.12m, 33, 64.61m)
                },
                CategoricalSpendingPerTimeStep = new()
                {
                    ("Loans", [105.20m, 107.40m, 106.10m, 104.80m]),
                    ("Groceries", [51.20m, 52.10m, 50.50m, 53.00m]),
                    ("Cheeses", [43.50m, 45.30m, 44.20m, 46.10m]),
                    ("Bills", [40.50m, 41.20m, 42.10m, 43.00m]),
                    ("Entertainment", [22.80m, 23.50m, 22.10m, 24.00m]),
                }
            });
        }

    }
}
