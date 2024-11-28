using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using SpendLess.Web.Domain.Services;

namespace SpendLess.Kvs.Controllers
{
    [Route("kvs")]
    public class KvsController(IComponentFactory componentFactory) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Kvs.Components.Kvs>();
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
