using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.NodeRed.Services;
using System.Text.Json;

namespace SpendLess.EventHandlers.NodeRed
{
    public class SendToNodeRedEventHandler(INodeRedService nodeRed, IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "NodeRedSend";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var body = requestData.Form.TryGetValue<string>("requestText");
            if (!body.HasValue)
                throw new InvalidOperationException("missing `requestText` parameter in form");

            var result = await nodeRed.SendToNodeRed(body.Value);

            try
            {
                var jsonDocument = JsonDocument.Parse(result);
                result = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
            }
            catch
            {
            }

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent(new NodeRedUpdateModel
                    {
                        ResponseText = new(result),
                    }),
                    await componentFactory.GetPlainComponent(new ToastModel
                    {
                        Message = "Sent to Node Red successfully.",
                        Severity = ToastSeverity.Success
                    })
                }
            });
        }
    }
}
