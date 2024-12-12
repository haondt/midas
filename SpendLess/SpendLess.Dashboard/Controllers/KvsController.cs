using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Dashboard.Controllers
{
    [Route("dashboard")]
    public class DashboardController(IComponentFactory componentFactory) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Dashboard.Components.Dashboard>();
        }

    }
}
