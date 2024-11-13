using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.NodeRed.Models;
using System.Text.Json;

namespace SpendLess.EventHandlers.NodeRed
{
    public class LoadDefaultNodeRedPayloadEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "NodeRedLoadDefault";

        public Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var prettifiedJson = JsonSerializer.Serialize(new SendToNodeRedRequestDto(), new JsonSerializerOptions
            {
                WriteIndented = true,
            });

            return componentFactory.GetPlainComponent(new NodeRedUpdateModel
            {
                RequestText = new(prettifiedJson)
            });
        }
    }
}
