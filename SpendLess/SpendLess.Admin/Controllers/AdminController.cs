using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Admin.Components;
using SpendLess.Admin.Services;
using SpendLess.Kvs.Models;
using SpendLess.Kvs.Services;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using SpendLess.Web.Domain.Extensions;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Admin.Controllers
{
    [Route("[controller]")]
    public class AdminController(IComponentFactory componentFactory,
        IDataService dataService,
        IKvsService kvs) : SpendLessUIController
    {
        [ServiceFilter(typeof(RenderPageFilter))]
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

        [HttpPost("takeout")]
        public async Task<IResult> ImportMappings()
        {
            var jobId = dataService.StartCreateTakeout();

            Response.AsResponseData()
                .HxPushUrl($"/admin/takeout/{jobId}");
            return await componentFactory.RenderComponentAsync(new Takeout
            {
                JobId = jobId
            });
        }

        [HttpGet("takeout/{jobId}")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> GetTakeout(string jobId)
        {
            var takeoutResult = dataService.GetAsyncJobResult(jobId);
            return await componentFactory.RenderComponentAsync(new Takeout
            {
                JobId = jobId,
            });
        }

        [HttpGet("takeout/{jobId}/poll")]
        public async Task<IResult> PollTakeout(string jobId)
        {
            var takeoutResult = dataService.GetAsyncJobResult(jobId);
            if (takeoutResult.IsSuccessful)
                return await componentFactory.RenderComponentAsync(new ReadyTakeout
                {
                    JobId = jobId,
                });
            Response.AsResponseData().HxReswap("none");
            return Results.Ok();
        }
    }
}
