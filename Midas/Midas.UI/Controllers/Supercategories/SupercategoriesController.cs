using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Supercategories.Services;
using Midas.UI.Components.Supercategories;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using System.ComponentModel.DataAnnotations;

namespace Midas.UI.Controllers.Supercategories
{
    [Route("supercategories")]
    public class SupercategoriesController(IComponentFactory componentFactory, ISupercategoryService supercategoryService) : MidasUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<SupercategoriesPanel>();
        }

        [HttpPost("map")]
        public async Task<IResult> UpsertMapping(
            [FromForm, Required(AllowEmptyStrings = false)] string category,
            [FromForm, Required(AllowEmptyStrings = false)] string supercategory)
        {
            await supercategoryService.UpsertMapping(category, supercategory);
            return await componentFactory.RenderComponentAsync<Components.Supercategories.Supercategories>();
        }

        [HttpDelete("map/{category}")]
        public async Task<IResult> DeleteMapping(string category)
        {
            await supercategoryService.DeleteMapping(category);
            return await componentFactory.RenderComponentAsync<Components.Supercategories.Supercategories>();
        }

        [HttpGet("search/supercategory")]
        public async Task<IResult> SearchSuperCategory([FromQuery] string? supercategory)
        {
            var results = await supercategoryService.SearchSupercategories(supercategory ?? "");
            return await componentFactory.RenderComponentAsync(new AutocompleteSuggestions
            {
                Suggestions = results
            });
        }
    }
}
