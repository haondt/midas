using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.EventHandlers
{
    public class NavigateEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "Navigate";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var target = requestData.Query.GetValue<string>("target");
            if (target == "accounts")
                return await componentFactory.GetPlainComponent(new AppendComponentOobSwapModel
                {
                    Items =
                    [
                        new() {
                            Component = await componentFactory.GetPlainComponent<AccountsModel>(),
                            TargetSelector = "#content"
                        },
                        new() {
                            Component = await componentFactory.GetPlainComponent(new SpendLessNavigationBarModel
                            {
                                ActiveItemTypeIdentity = ComponentDescriptor<AccountsModel>.TypeIdentity
                            }),
                            TargetSelector = "#content"
                        },
                    ]
                });

            throw new InvalidOperationException($"Unknown target {target}");
        }
    }
}
