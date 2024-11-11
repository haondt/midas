using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using SpendLess.Components.Abstractions;
using SpendLess.Components.Exceptions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Services;

namespace SpendLess.Components.SpendLessComponents
{
    public class UpsertAccountModel : IComponentModel
    {
        public required Optional<StorageKey<AccountDto>> AccountId { get; set; }
        public string AccountName { get; set; } = "";
    }

    public class UpsertAccountComponentDescriptorFactory(ISpendLessStorage storage) : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<UpsertAccountModel>(async (cf, rd) =>
            {
                var id = rd.Query.TryGetValue<string>("id");
                if (!id.HasValue)
                    return new UpsertAccountModel
                    {
                        AccountId = new()
                    };

                if (!Guid.TryParse(id.Value, out var guid))
                    throw new NotFoundPageException();

                var accountKey = AccountDto.GetStorageKey(guid);
                var account = await storage.Get(accountKey);
                if (!account.IsSuccessful)
                    throw new NotFoundPageException();

                return new UpsertAccountModel
                {
                    AccountId = accountKey,
                    AccountName = account.Value.Name
                };
            })
            {
                ViewPath = $"~/SpendLessComponents/UpsertAccount.cshtml"
            };
        }
    }
}
