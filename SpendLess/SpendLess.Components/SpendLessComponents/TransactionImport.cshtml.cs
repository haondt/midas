using Haondt.Core.Models;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportModel
    {
        public Dictionary<string, string> Accounts { get; set; } = [];
        public Dictionary<string, string> ImportConfigurations { get; set; } = [];
        public Optional<(string Id, string Name)> SelectedImportConfiguration { get; set; } = new();
    }

    //public class TransactionImportComponentDescriptorFactory(
    //    ISingleTypeSpendLessStorage<AccountDto> accountStorage,
    //    ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
    //    IStorage storage) : IComponentDescriptorFactory
    //{
    //    public IComponentDescriptor Create()
    //    {
    //        return new ComponentDescriptor<TransactionImportModel>(async () =>
    //        {
    //            var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
    //            var accounts = (await accountStorage.GetAll()).ToDictionary(
    //                    kvp => kvp.Key.SingleValue(),
    //                    kvp => kvp.Value.Name);
    //            var model = new TransactionImportModel { Accounts = accounts };
    //            return model;
    //        })
    //        {
    //            ViewPath = $"~/SpendLessComponents/TransactionImport.cshtml"
    //        };
    //    }
    //}
}
