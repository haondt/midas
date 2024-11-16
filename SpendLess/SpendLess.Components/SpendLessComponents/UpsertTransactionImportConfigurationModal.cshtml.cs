using Haondt.Core.Models;
using Haondt.Persistence.Services;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class UpsertTransactionImportConfigurationModalModel : IComponentModel
    {
        public Optional<string> Id { get; set; } = new();
        public string Name { get; set; } = "";
        public bool AddTimestampTag { get; set; } = true;
        public bool IsDefault { get; set; } = false;
    }

    public class UpsertTransactionImportConfigurationModalComponentDescriptorFactory(
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        IStorage storage) : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<UpsertTransactionImportConfigurationModalModel>(async (cf, rd) =>
            {
                var id = rd.Query.TryGetValue<string>("config-id");
                if (id.HasValue)
                {
                    var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
                    var configKey = id.Value.SeedStorageKey<TransactionImportConfigurationDto>();
                    var config = await configStorage.Get(configKey);
                    var isDefault = state.DefaultImportConfigurationKey.HasValue
                        && state.DefaultImportConfigurationKey.Value == configKey;

                    return new UpsertTransactionImportConfigurationModalModel
                    {
                        Id = id,
                        Name = config.Name,
                        IsDefault = isDefault,
                        AddTimestampTag = config.AddTagWithCurrentDateTimeToAllTransactions
                    };
                }
                else
                {
                    var isDefault = rd.Query.TryGetValue<string>("is-default");
                    return new UpsertTransactionImportConfigurationModalModel
                    {
                        IsDefault = isDefault.HasValue
                    };
                }
            })
            {
                ViewPath = $"~/SpendLessComponents/UpsertTransactionImportConfigurationModal.cshtml"
            };
        }
    }
}
