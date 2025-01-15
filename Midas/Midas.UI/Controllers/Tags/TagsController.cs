using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Tags
{
    [Route("[controller]")]
    public class TagsController(IComponentFactory componentFactory) : MidasUIController
    {
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Midas.UI.Components.Tags.Tags>();
        }
    }
}
