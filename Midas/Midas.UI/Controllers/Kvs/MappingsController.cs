using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Kvs.Services;
using Midas.UI.Components.Kvs;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;

namespace Midas.Kvs.Controllers
{
    [Route("kvs/mappings")]
    public class MappingsController(IComponentFactory componentFactory,
        IKvsService kvs) : MidasUIController
    {

        [HttpGet]
        public Task<IResult> LaunchMappingsModal()
        {
            return componentFactory.RenderComponentAsync<SelectMappingModal>();
        }

        [HttpGet("search")]
        public async Task<IResult> SearchMapping([FromQuery] string? query)
        {
            var suggestions = await kvs.Search(query ?? "");
            return await componentFactory.RenderComponentAsync(new AutocompleteSuggestions
            {
                Suggestions = suggestions
            });
        }
    }
}
