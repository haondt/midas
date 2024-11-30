using Haondt.Identity.StorageKey;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.TransactionImport.Components;
using SpendLess.TransactionImport.Models;
using SpendLess.TransactionImport.Services;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using SpendLess.Web.Domain.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.TransactionImport.Controllers
{
    [Route("transaction-import")]
    public class TransactionImportController(
        IComponentFactory componentFactory,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        ISingleTypeSpendLessStorage<TransactionImportAccountMetadataDto> accountMetadataStorage,
        ITransactionImportService import) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<TransactionImport.Components.TransactionImport>();
        }

        [HttpGet("config")]
        public async Task<IResult> GetConfigModal(
            [FromQuery] string? config,
            [FromQuery] string? account)
        {
            var model = new UpsertConfigurationModal();

            if (!string.IsNullOrEmpty(config))
            {
                model.Id = config;
                var configDto = await configStorage.Get(config.SeedStorageKey<TransactionImportConfigurationDto>());
                model.Name = configDto.Name;
                model.AddImportTag = configDto.AddImportTag;

            }
            if (!string.IsNullOrEmpty(account))
            {
                var accountMetadata = await accountMetadataStorage.TryGet(account.SeedStorageKey<TransactionImportAccountMetadataDto>());
                var isDefault = accountMetadata.HasValue
                    && accountMetadata.Value.DefaultConfiguration.HasValue
                    && accountMetadata.Value.DefaultConfiguration.Value.SingleValue() == config;

                model.SelectedAccount = new((account, isDefault));

            }
            return await componentFactory.RenderComponentAsync(model);
        }


        [HttpPost("{config}")]
        public async Task<IResult> UpsertConfig([FromForm] TransactionImportUpsertConfigRequestDto request)
        {
            request.Name = request.Name.Trim();
            request.Id ??= Guid.NewGuid().ToString();
            var configStorageKey = request.Id.SeedStorageKey<TransactionImportConfigurationDto>();

            var updatedConfig = new TransactionImportConfigurationDto
            {
                AddImportTag = request.AddImportTag,
                Name = request.Name,
            };
            await configStorage.Set(configStorageKey, updatedConfig);

            if (!string.IsNullOrEmpty(request.Account))
            {
                var accountStorageKey = request.Account.SeedStorageKey<TransactionImportAccountMetadataDto>();
                await accountMetadataStorage.Set(accountStorageKey, new TransactionImportAccountMetadataDto
                {
                    DefaultConfiguration = configStorageKey
                });
            }

            var setupModel = new Setup();
            if (!string.IsNullOrEmpty(request.Account))
                setupModel.SelectedAccount = request.Account;
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new CloseModal(),
                    setupModel
                ]
            });
        }

        [HttpPost("dry-run")]
        public async Task<IResult> StartDryRun(
            [FromForm, Required] string account,
            [FromForm, Required] string config,
            [FromForm, Required] IFormFile file)
        {
            var csvData = file.ParseAsCsv();
            var configDto = await configStorage.Get(config.SeedStorageKey<TransactionImportConfigurationDto>());
            var jobId = import.StartDryRun(configDto, account, csvData);

            return await componentFactory.RenderComponentAsync(new ProgressPanel
            {
                CallbackEndpoint = $"/transaction-import/dry-run/{jobId}",
                ProgressPercent = 0
            });
        }

        [HttpGet("dry-run/{id}")]
        public async Task<IResult> GetJobStatus(string id)
        {
            var result = import.GetDryRunResult(id);
            if (!result.IsSuccessful)
            {
                return await componentFactory.RenderComponentAsync(new ProgressPanel
                {
                    CallbackEndpoint = $"/transaction-import/dry-run/{id}",
                    ProgressPercent = result.Reason
                });
            }

            return await componentFactory.RenderComponentAsync(new DryRunResult { Inner = result.Value });
        }
    }

}
