using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Models;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;

namespace Midas.Kvs.Controllers
{
    [Route("kvs")]
    public class KvsController(IComponentFactory componentFactory) : MidasUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Midas.UI.Components.Kvs.Kvs>();
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
