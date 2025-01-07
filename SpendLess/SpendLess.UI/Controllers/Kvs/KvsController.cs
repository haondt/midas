using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Core.Models;
using SpendLess.UI.Shared.Components;
using SpendLess.UI.Shared.Controllers;

namespace SpendLess.Kvs.Controllers
{
    [Route("kvs")]
    public class KvsController(IComponentFactory componentFactory) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<SpendLess.UI.Components.Kvs.Kvs>();
        }

        [HttpPost("prettify")]
        public Task<IResult> Prettify([FromForm] string value)
        {
            return componentFactory.RenderComponentAsync(new CodeWindow
            {
                Name = "value",
                Text = StringFormatter.TryPrettify(value)
            });
        }
    }
}
