using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using System.Text.Json;

namespace SpendLess.EventHandlers.NodeRed
{
    public class PrettifyRequestEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "NodeRedPrettify";

        public Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var lookupValue = requestData.Form.GetValue<string>("requestText");

            string prettifiedJson;
            try
            {
                var jsonDocument = JsonDocument.Parse(lookupValue);
                prettifiedJson = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
            }
            catch
            {
                prettifiedJson = lookupValue;
            }

            return componentFactory.GetPlainComponent(new NodeRedUpdateModel
            {
                RequestText = new(prettifiedJson)
            });
        }
    }
}
