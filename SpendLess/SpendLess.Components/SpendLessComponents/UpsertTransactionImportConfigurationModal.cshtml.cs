using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using SpendLess.Components.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class UpsertTransactionImportConfigurationModalModel : IComponentModel
    {
        public Optional<string> Id { get; set; } = new();
        public string Name { get; set; } = "";
        public bool AddTimestampTag { get; set; } = true;
        public bool IsDefault { get; set; } = false;
    }

    public class UpsertTransactionImportConfigurationModalComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<UpsertTransactionImportConfigurationModalModel>((cf, rd) =>
            {
                var id = rd.Query.TryGetValue<string>("id");
                if (id.HasValue)
                {
                    throw new NotImplementedException();
                }

                var isDefault = rd.Query.TryGetValue<string>("is-default");
                return new UpsertTransactionImportConfigurationModalModel
                {
                    IsDefault = isDefault.HasValue
                };
            })
            {
                ViewPath = $"~/SpendLessComponents/UpsertTransactionImportConfigurationModal.cshtml"
            };
        }
    }
}
