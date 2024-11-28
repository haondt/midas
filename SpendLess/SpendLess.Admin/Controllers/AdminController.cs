using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Kvs.Models;
using SpendLess.Kvs.Services;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Extensions;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Admin.Controllers
{
    [Route("[controller]")]
    public class AdminController(IComponentFactory componentFactory,
        IKvsService kvs) : UIController
    {
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Admin.Components.Admin>();
        }

        [HttpPost("import-mappings")]
        public async Task<IResult> ImportMappings([FromForm] IFormFile file,
            [FromForm(Name = "overwrite-existing")] bool overwriteExisting)
        {
            var parsedData = file.DeserializeFromJson<ExternalKvsMappingsDto>();

            await kvs.ImportKvsMappings(parsedData, overwriteExisting);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Imported mappings successfully!",
                Severity = ToastSeverity.Success,
            });
        }
    }
}
