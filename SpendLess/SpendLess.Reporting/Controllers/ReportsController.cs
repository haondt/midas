using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Reporting.Components;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Reporting.Controllers
{
    [Route("reports")]
    public class ReportsController(IComponentFactory componentFactory) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get()
        {
            return await componentFactory.RenderComponentAsync<GenerateReport>(new());
        }
    }
}
