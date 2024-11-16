using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.EventHandlers.Accounts
{
    public class AccountUpsertEventHandler(IComponentFactory componentFactory,
        ISingleTypeSpendLessStorage<AccountDto> storage) : ISingleEventHandler
    {
        public string EventName => "AccountUpsert";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var accountIdFromForm = requestData.Form.TryGetValue<string>("id");
            var accountName = requestData.Form.TryGetValue<string>("name");

            if (!accountName.HasValue || string.IsNullOrEmpty(accountName.Value))
                throw new UserException("Account name cannot be empty");

            var accountId = accountIdFromForm.HasValue
                ? accountIdFromForm.Value
                : Guid.NewGuid().ToString();
            await storage.Set(accountId.SeedStorageKey<AccountDto>(), new AccountDto
            {
                Balance = 0,
                Name = accountName.Value,
            });

            var toastMessage = accountIdFromForm.HasValue
                ? "Updated account successfully."
                : "Created account successfully.";

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent(new AppendComponentOobSwapModel
                    {
                        Items = new List<OobSwap>
                        {
                            new() {
                                Component = await componentFactory.GetPlainComponent(new UpsertAccountModel
                                {
                                    AccountId = accountId,
                                    AccountName = accountName.Value
                                }),
                                TargetSelector = "#content"
                            }
                        }
                    }),
                    await componentFactory.GetPlainComponent(new ToastModel
                    {
                        Message = toastMessage,
                        Severity = ToastSeverity.Success
                    })
                }
            }, configureResponse: m => m.ConfigureHeadersAction = new HxHeaderBuilder()
                .PushUrl($"/account/{accountId}")
                .Build());
        }

    }
}
