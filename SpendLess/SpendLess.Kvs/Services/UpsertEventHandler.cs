using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Kvs.Services
{
    public class UpsertEventHandler(IComponentFactory componentFactory,
        IKvsService kvsService) : ISingleEventHandler
    {
        public string EventName => "KvsUpsert";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var value = requestData.Form.GetValue<string>("value");
            var key = requestData.Form.GetValue<string>("key");

            if (key == "")
                throw new InvalidOperationException("key cannot be empty");

            await kvsService.UpsertValue(key, value);

            return await componentFactory.GetPlainComponent(new ToastModel
            {
                Message = $"Updated value for {key}.",
                Severity = ToastSeverity.Success
            });
        }
    }
}
