using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using System.Text.Json;

namespace SpendLess.EventHandlers.NodeRed
{
    public class LoadDefaultNodeRedPalyoadEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "NodeRedLoadDefault";

        public Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var defaultValue = new
            {
                account = "",
                csv = Array.Empty<object>()
            };
            var prettifiedJson = JsonSerializer.Serialize(defaultValue, new JsonSerializerOptions
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
