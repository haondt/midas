using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Core.Exceptions;
using SpendLess.Kvs.SpendLess.Kvs;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Kvs.Services
{
    public class AddAliasEventHandler(IComponentFactory componentFactory, IKvsService kvs) : ISingleEventHandler
    {
        public string EventName => "KvsAddAlias";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            if (!requestData.Form.TryGetValue<string>("alias", out var alias) || string.IsNullOrWhiteSpace(alias))
                throw new UserException("Alias cannot be empty.");

            var key = requestData.Form.GetValue<string>("key");
            var aliases = await kvs.AddAlias(key, alias);

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent(new ToastModel
                    {
                        Message = "Alias added!",
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
