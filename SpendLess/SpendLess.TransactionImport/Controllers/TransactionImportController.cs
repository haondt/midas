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
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.TransactionImport.Controllers
{
    [Route("transaction-import")]
    public class TransactionImportController(
        IComponentFactory componentFactory,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        ISingleTypeSpendLessStorage<TransactionImportAccountMetadataDto> accountMetadataStorage
        ) : SpendLessUIController
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

    }

    //public class Test : TransactionImportController
    //{
    //    [HttpGet]
    //    public async Task<IResult> Get()
    //    {
    //        return new RazorComponentResult<TestOne>(new TestOne
    //        {
    //            Color = "blue",
    //            Size = 100
    //        });
    //    }

    //}
}
