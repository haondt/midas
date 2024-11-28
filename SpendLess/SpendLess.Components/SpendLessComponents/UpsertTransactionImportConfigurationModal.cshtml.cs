using Haondt.Core.Models;

namespace SpendLess.Components.SpendLessComponents
{
    public class UpsertTransactionImportConfigurationModalModel
    {
        public Optional<string> Id { get; set; } = new();
        public string Name { get; set; } = "";
        public bool AddTimestampTag { get; set; } = true;
        public bool IsDefault { get; set; } = false;
    }

    //public class UpsertTransactionImportConfigurationModalComponentDescriptorFactory(
    //    ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
    //    IStorage storage) : IComponentDescriptorFactory
    //{
    //    public IComponentDescriptor Create()
    //    {
    //        return new ComponentDescriptor<UpsertTransactionImportConfigurationModalModel>(async (cf, rd) =>
    //        {
    //            var id = rd.Query.TryGetValue<string>("config-id");
    //            if (id.HasValue)
    //            {
    //                var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
    //                var configKey = id.Value.SeedStorageKey<TransactionImportConfigurationDto>();
    //                var config = await configStorage.Get(configKey);
    //                return new UpsertTransactionImportConfigurationModalModel
    //                {
    //                    Id = id,
    //                    Name = config.Name,
    //                    AddTimestampTag = config.AddTagWithCurrentDateTimeToAllTransactions
    //                };
    //            }
    //            else
    //            {
    //                var isDefault = rd.Query.TryGetValue<string>("is-default");
    //                return new UpsertTransactionImportConfigurationModalModel
    //                {
    //                    IsDefault = isDefault.HasValue
    //                };
    //            }
    //        })
    //        {
    //            ViewPath = $"~/SpendLessComponents/UpsertTransactionImportConfigurationModal.cshtml"
    //        };
    //    }
    //}
}
