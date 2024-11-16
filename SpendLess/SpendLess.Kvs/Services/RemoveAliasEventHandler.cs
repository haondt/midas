using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Kvs.SpendLess.Kvs;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Kvs.Services
{
    public class RemoveAliasEventHandler(IComponentFactory componentFactory, IKvsService kvs) : ISingleEventHandler
    {
        public string EventName => "KvsRemoveAlias";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var key = requestData.Form.GetValue<string>("key");
            var alias = requestData.Form.GetValue<string>("alias");
            var aliases = await kvs.RemoveAlias(key, alias);

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent(new ToastModel
                    {
                        Message = "Alias removed",
                        Severity = ToastSeverity.Success
                    }),
                    await componentFactory.GetPlainComponent<KvsModel>(new KvsUpdateContentModel
                    {
                        Aliases = aliases
                    })
                }
            });
        }
    }
}
