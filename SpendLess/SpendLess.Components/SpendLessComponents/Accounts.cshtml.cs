using Haondt.Identity.StorageKey;
using Haondt.Web.Core.Components;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Services;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class AccountsModel : IComponentModel
    {
        public Dictionary<string, AccountDto> Accounts { get; set; } = [];
    }

    public class AccountsComponentDescriptorFactory(ISingleTypeSpendLessStorage<AccountDto> storage) : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<AccountsModel>(async () =>
            {
                var accounts = (await storage.GetAll())
                    .ToDictionary(kvp => kvp.Key.SingleValue<string>(),
                    kvp => kvp.Value);
                return new AccountsModel
                {
                    Accounts = accounts
                };
            })
            {
                ViewPath = $"~/SpendLessComponents/Accounts.cshtml"
            };
        }
    }
}
