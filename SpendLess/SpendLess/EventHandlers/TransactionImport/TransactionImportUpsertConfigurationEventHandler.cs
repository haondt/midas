using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.EventHandlers.TransactionImport
{
    public class TransactionImportUpsertConfigurationEventHandler(IComponentFactory componentFactory,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        IStorage storage) : ISingleEventHandler
    {
        public string EventName => "TransactionImportUpsertConfiguration";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var id = requestData.Form.TryGetValue<string>("id");
            var name = requestData.Form.TryGetValue<string>("name");
            if (!name.HasValue || string.IsNullOrEmpty(name.Value))
                throw new UserException($"Name is required.");

            var addCurrentDateTag = requestData.Form.TryGetValue<string>("add-ts-tag").HasValue;
            var isDefault = requestData.Form.TryGetValue<string>("is-default").HasValue;

            var configurationId = id.HasValue ? id.Value : Guid.NewGuid().ToString();
            var configurationStorageKey = configurationId.SeedStorageKey<TransactionImportConfigurationDto>();
            await configStorage.Set(configurationId.SeedStorageKey<TransactionImportConfigurationDto>(), new TransactionImportConfigurationDto
            {
                AddTagWithCurrentDateTimeToAllTransactions = addCurrentDateTag,
                Name = name.Value
            });

            var importConfigs = await configStorage.GetAll();
            isDefault |= (importConfigs.Count == 1);
            if (isDefault)
            {
                var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
                state.DefaultImportConfigurationKey = configurationStorageKey;
                await storage.Set(SpendLessStateDto.StorageKey, state);
            }

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent<CloseModalModel>(),
                    await componentFactory.GetPlainComponent(
                        new TransactionImportModel
                        {
                            SelectedImportConfiguration = (configurationId, name.Value),
                            ImportConfigurations = importConfigs.ToDictionary(
                                kvp => kvp.Key.SingleValue(),
                                kvp => kvp.Value.Name)

                        })
                }
            },
            configureResponse: m => m.ConfigureHeadersAction = new HxHeaderBuilder()
            .ReSelect("#import-configuration-selection-field")
            .ReTarget("#import-configuration-selection-field")
            .ReSwap("true")
            .Build());
        }
    }
}
