using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Web.Core.Abstractions;
using SpendLess.Web.Core.Exceptions;

namespace SpendLess.Components.SpendLessComponents
{
    public class UpsertAccountModel : IComponentModel
    {
        public required Optional<string> AccountId { get; set; }
        public string AccountName { get; set; } = "";
    }

    public class UpsertAccountComponentDescriptorFactory(ISingleTypeSpendLessStorage<AccountDto> storage) : IComponentDescriptorFactory
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

                var account = await storage.TryGet(id.Value.SeedStorageKey<AccountDto>());
                if (!account.HasValue)
                    throw new NotFoundPageException();

                return new UpsertAccountModel
                {
                    AccountId = id.Value,
                    AccountName = account.Value.Name
                };
            })
            {
                ViewPath = $"~/SpendLessComponents/UpsertAccount.cshtml"
            };
        }
    }
}
