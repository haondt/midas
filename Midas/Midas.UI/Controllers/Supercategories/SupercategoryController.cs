using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Supercategories.Services;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Supercategories
{
    [Route("supercategory")]
    public class SupercategoryController(IComponentFactory componentFactory, ISupercategoryService supercategoryService) : MidasUIController
    {
        [HttpDelete("{supercategory}")]
        public async Task<IResult> Delete(string supercategory)
        {
            await supercategoryService.DeleteSupercategory(supercategory);
            return await componentFactory.RenderComponentAsync<Components.Supercategories.Supercategories>();
        }
    }
}
