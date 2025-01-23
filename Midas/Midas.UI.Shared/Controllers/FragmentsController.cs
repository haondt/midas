using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Exceptions;
using Midas.UI.Shared.Components;

namespace Midas.UI.Shared.Controllers
{
    [Route("fragments")]
    public class FragmentsController(IComponentFactory componentFactory) : MidasUIController
    {
        [HttpGet("tag")]
        public Task<IResult> GetTag([FromQuery] string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new UserException("Tag cannot be empty");
            return componentFactory.RenderComponentAsync(new Tag { Text = tag.Trim() });
        }
    }
}
