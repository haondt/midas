using Haondt.Core.Models;
using Haondt.Persistence.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;

namespace SpendLess.EventHandlers.TransactionImport
{
    public class TransactionImportUpsertConfigurationEventHandler(IComponentFactory componentFactory,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        IStorage storage) : ISingleEventHandler
    {
        public string EventName => "TransactionImportUpsertConfiguration";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var id = requestData.Form.TryGetValue<Guid>("id");
            var name = requestData.Form.TryGetValue<string>("name");
            if (!name.HasValue || string.IsNullOrEmpty(name.Value))
                throw new UserException($"Name is required.");

            var addCurrentDateTag = requestData.Form.TryGetValue<string>("add-ts-tag").HasValue;
            var isDefault = requestData.Form.TryGetValue<string>("is-default").HasValue;

            var configurationId = id.HasValue ? id.Value : Guid.NewGuid();
            var configurationStorageKey = configurationId.SeedStorageKey<TransactionImportConfigurationDto>();
            await configStorage.Set(configurationId.SeedStorageKey<TransactionImportConfigurationDto>(), new TransactionImportConfigurationDto
            {
                AddTagWithCurrentDateTimeToAllTransactions = addCurrentDateTag,
                Name = name.Value
            });

            var importConfigs = await configStorage.GetAll();
            isDefault |= (importConfigs.Count == 1);
            var defaultImportConfigurationInfo = new Optional<(string Id, string Name)>();
            var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
            if (isDefault)
            {
                defaultImportConfigurationInfo = (configurationId.ToString(), name.Value);
                state.DefaultImportConfigurationKey = configurationStorageKey;
                await storage.Set(SpendLessStateDto.StorageKey, state);
            }
            else if (state.DefaultImportConfigurationKey.HasValue)
            {
                if (importConfigs.TryGetValue(state.DefaultImportConfigurationKey.Value, out var defaultConfiguration))
                    defaultImportConfigurationInfo = (state.DefaultImportConfigurationKey.Value.SingleValue(), defaultConfiguration.Name);
            }

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent<CloseModalModel>(),
                    await componentFactory.GetPlainComponent(
                        new TransactionImportModel
                        {
                            DefaultImportConfiguration = defaultImportConfigurationInfo,
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
