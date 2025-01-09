using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.UI.Components.Reporting;
using SpendLess.UI.Shared.Controllers;

namespace SpendLess.UI.Controllers.Reporting
{
    [Route("reports")]
    public class ReportsController(IComponentFactory componentFactory) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get()
        {
            return await componentFactory.RenderComponentAsync<GenerateReport>();
        }
    }
}
