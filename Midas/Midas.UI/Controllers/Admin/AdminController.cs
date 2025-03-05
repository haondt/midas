using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Admin.Models;
using Midas.Domain.Admin.Services;
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
        IDataService dataService) : MidasUIController
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
            var parsedData = file.DeserializeFromJson<TakeoutKvsMappingsDto>();

            await dataService.ImportKvsMappings(parsedData, overwriteExisting);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Imported mappings successfully!",
                Severity = ToastSeverity.Success,
            });
        }

        [HttpPost("import-accounts")]
        public async Task<IResult> ImportAccounts([FromForm] IFormFile file,
            [FromForm(Name = "overwrite-existing"), ModelBinder(typeof(CheckboxModelBinder))] bool overwriteExisting)
        {
            var parsedData = file.DeserializeFromJson<TakeoutAccountsDto>();

            await dataService.ImportAccounts(parsedData, overwriteExisting);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Imported accounts successfully!",
                Severity = ToastSeverity.Success,
            });
        }

        [HttpPost("import-transactions")]
        public async Task<IResult> ImportTransactions([FromForm] IFormFile file)
        {
            var parsedData = file.DeserializeFromJson<TakeoutTransactionsDto>();

            await dataService.ImportTransactions(parsedData);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Imported transactions successfully!",
                Severity = ToastSeverity.Success,
            });
        }

        [HttpPost("import-import-configurations")]
        public async Task<IResult> ImportTransactionImportConfigurations([FromForm] IFormFile file)
        {
            var parsedData = file.DeserializeFromJson<TakeoutImportConfigurationsDto>();

            await dataService.ImportTransactionImportConfigurations(parsedData);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Imported configurations successfully!",
                Severity = ToastSeverity.Success,
            });
        }

        [HttpPost("import-supercategories")]
        public async Task<IResult> ImportSupercategories([FromForm] IFormFile file,
            [FromForm(Name = "overwrite-existing"), ModelBinder(typeof(CheckboxModelBinder))] bool overwriteExisting)
        {
            var parsedData = file.DeserializeFromJson<TakeoutSupercategoriesDto>();

            await dataService.ImportSupercategories(parsedData, overwriteExisting);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Imported supercategories successfully!",
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

        [HttpDelete("bulk/mappings")]
        public async Task<IResult> DeleteAllMappings()
        {
            var deleted = await dataService.DeleteAllMappings();

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Severity = ToastSeverity.Success,
                Message = $"Deleted {deleted} mappings."
            });
        }
    }
}
