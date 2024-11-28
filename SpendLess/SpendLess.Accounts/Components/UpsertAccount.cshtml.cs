using Haondt.Core.Models;

namespace SpendLess.Accounts.Components
{
    public class UpsertAccountModel
    {
        public required Optional<string> AccountId { get; set; }
        public string AccountName { get; set; } = "";
    }

    //public class UpsertAccountComponentDescriptorFactory(ISingleTypeSpendLessStorage<AccountDto> storage) : IComponentDescriptorFactory
    //{
    //    public IComponentDescriptor Create()
    //    {
    //        return new ComponentDescriptor<UpsertAccountModel>(async (cf, rd) =>
    //        {
    //            var id = rd.Query.TryGetValue<string>("id");
    //            if (!id.HasValue)
    //                return new UpsertAccountModel
    //                {
    //                    AccountId = new()
    //                };

    //            var account = await storage.TryGet(id.Value.SeedStorageKey<AccountDto>());
    //            if (!account.HasValue)
    //                throw new NotFoundPageException();

    //            return new UpsertAccountModel
    //            {
    //                AccountId = id.Value,
    //                AccountName = account.Value.Name
    //            };
    //        })
    //        {
    //            ViewPath = $"~/SpendLessComponents/UpsertAccount.cshtml"
    //        };
    //    }
    //}
}
