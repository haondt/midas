using Haondt.Persistence.Services;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Services;

namespace SpendLess.TransactionImport.Services
{
    public class TransactionImportComponentDescriptorFactory(
        ISingleTypeSpendLessStorage<AccountDto> accountStorage,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        IStorage storage)
    {
        //public IComponentDescriptor Create()
        //{
        //    return new ComponentDescriptor<TransactionImportModel>(ComponentFactory)
        //    {
        //        ViewPath = $"~/SpendLess.TransactionImport/TransactionImport.cshtml"
        //    };
        //}

        //private async Task<TransactionImportModel> ComponentFactory(IComponentFactory componentFactory, IRequestData requestData)
        //{
        //    if (requestData.Query.TryGetValue<bool>("launch-modal", out var launchModal) && launchModal)
        //    {
        //        var account = await requestData.Query.TryGetValue<string>("account")
        //            .ConvertAsync(async a => new { Id = a, Value = await accountStorage.Get(a.SeedStorageKey<AccountDto>()) });
        //        var create = requestData.Query.TryGetValue<bool>("create").Or(false);

        //        var model = new TransactionImportEditConfigurationModel
        //        {
        //            IsCreate = create,
        //            SelectedAccount = account.Convert(a => a.Id)
        //        };

        //        if (account.HasValue)
        //            model.SelectedAccount = account.Value.Id;

        //        if (create)
        //        {
        //            if (account.HasValue)
        //                model.IsDefaultForSelectedAccount = !account.Value.Value.DefaultImportConfiguration.HasValue;
        //            return model;
        //        }

        //        var configId = requestData.Query.GetValue<string>("config");
        //        var config = await configStorage.Get(configId.SeedStorageKey<TransactionImportConfigurationDto>());
        //        model.AddTagWithCurrentDateTimeToAllTransactions = config.AddTagWithCurrentDateTimeToAllTransactions;
        //        model.Name = config.Name;
        //        model.Id = configId;

        //        if (account.HasValue)
        //            model.IsDefaultForSelectedAccount = account.Value.Value.DefaultImportConfiguration.HasValue
        //                && account.Value.Value.DefaultImportConfiguration.Value.SingleValue() == configId;
        //        return model;
        //    }

        //    var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
        //    var accounts = await accountStorage.GetAll();
        //    var importConfigs = await configStorage.GetAll();

        //    return new TransactionImportLayoutModel
        //    {
        //        Setup = new TransactionImportSetupModel
        //        {
        //            Accounts = accounts.ToDictionary(kvp => kvp.Key.SingleValue(),
        //                kvp => (kvp.Value.Name, kvp.Value.DefaultImportConfiguration.Convert(k => k.SingleValue()))),
        //            Configurations = importConfigs.ToDictionary(kvp => kvp.Key.SingleValue(),
        //                kvp => kvp.Value.Name)
        //        }
        //    };

        //}
    }
}
