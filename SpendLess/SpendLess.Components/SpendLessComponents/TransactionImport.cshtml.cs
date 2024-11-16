using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Web.Core.Components;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportModel : IComponentModel
    {
        public Dictionary<string, string> Accounts { get; set; } = [];
        public Dictionary<string, string> ImportConfigurations { get; set; } = [];
        public Optional<(string Id, string Name)> SelectedImportConfiguration { get; set; } = new();
    }

    public class TransactionImportComponentDescriptorFactory(
        ISingleTypeSpendLessStorage<AccountDto> accountStorage,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        IStorage storage) : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<TransactionImportModel>(async () =>
            {
                var state = await storage.GetDefault(SpendLessStateDto.StorageKey);
                var accounts = (await accountStorage.GetAll()).ToDictionary(
                        kvp => kvp.Key.SingleValue(),
                        kvp => kvp.Value.Name);
                var model = new TransactionImportModel { Accounts = accounts };
                if (!state.DefaultImportConfigurationKey.HasValue)
                    return model;

                var importConfigs = await configStorage.GetAll();
                if (!importConfigs.TryGetValue(state.DefaultImportConfigurationKey.Value, out var defaultImportConfiguration))
                    return model;

                model.SelectedImportConfiguration = (state.DefaultImportConfigurationKey.Value.SingleValue(), defaultImportConfiguration.Name);
                model.ImportConfigurations = importConfigs.ToDictionary(
                    kvp => kvp.Key.SingleValue(),
                    kvp => kvp.Value.Name);
                return model;
            })
            {
                ViewPath = $"~/SpendLessComponents/TransactionImport.cshtml"
            };
        }
    }
}
