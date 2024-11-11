using Haondt.Identity.StorageKey;
using Haondt.Web.Core.Components;
using SpendLess.Components.Abstractions;
using SpendLess.Domain.Models;
using SpendLess.Domain.Services;

namespace SpendLess.Components.SpendLessComponents
{
    public class AccountsModel : IComponentModel
    {
        public Dictionary<StorageKey<AccountDto>, AccountDto> Accounts { get; set; } = [];
    }

    public class AccountsComponentDescriptorFactory(IAccountService accountService) : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<AccountsModel>(async () =>
            {
                var accounts = await accountService.GetAccounts();
                return new AccountsModel
                {
                    Accounts = await accountService.GetAccounts()
                };
            })
            {
                ViewPath = $"~/SpendLessComponents/Accounts.cshtml"
            };
        }
    }
}
