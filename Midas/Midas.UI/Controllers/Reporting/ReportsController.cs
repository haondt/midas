using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.UI.Components.Reporting;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Reporting
{
    [Route("reports")]
    public class ReportsController(IComponentFactory componentFactory) : MidasUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get()
        {
            return await componentFactory.RenderComponentAsync<GenerateReport>();
        }
    }
}
