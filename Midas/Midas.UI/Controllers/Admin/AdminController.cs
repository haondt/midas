﻿using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Admin.Services;
using Midas.Domain.Kvs.Models;
using Midas.Domain.Kvs.Services;
using Midas.UI.Components.Admin;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using Midas.UI.Shared.Extensions;
using Midas.UI.Shared.ModelBinders;
using Midas.UI.Shared.Models;

namespace Midas.Admin.Controllers
{
    [Route("[controller]")]
    public class AdminController(IComponentFactory componentFactory,
        IDataService dataService,
        IKvsService kvs) : MidasUIController
    {
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Midas.UI.Components.Admin.Admin>();
        }

        [HttpPost("import-mappings")]
        public async Task<IResult> ImportMappings([FromForm] IFormFile file,
            [FromForm(Name = "overwrite-existing"), ModelBinder(typeof(CheckboxModelBinder))] bool overwriteExisting)
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

        [HttpDelete("bulk/transactions")]
        public async Task<IResult> DeleteAllTransactions()
        {
            var deleted = await dataService.DeleteAllTransactions();

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Severity = ToastSeverity.Success,
                Message = $"Deleted {deleted} transactions."
            });
        }

        [HttpDelete("bulk/accounts")]
        public async Task<IResult> DeleteAllAccounts()
        {
            var deleted = await dataService.DeleteAllAccounts();

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Severity = ToastSeverity.Success,
                Message = $"Deleted {deleted} accounts."
            });
        }
    }
}