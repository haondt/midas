using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Exceptions;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Models;

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

        [HttpGet("toast")]
        public Task<IResult> GetToast([FromQuery] string message, [FromQuery] ToastSeverity severity)
        {
            return componentFactory.RenderComponentAsync(new Toast
            {
                Message = message,
                Severity = severity
            });
        }
    }
}
