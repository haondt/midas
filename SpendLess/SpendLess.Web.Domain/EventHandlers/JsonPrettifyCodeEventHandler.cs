using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;
using System.Text.Json;

namespace SpendLess.EventHandlers.SpendLess
{
    public class JsonPrettifyCodeEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "JsonPrettifyCode";

        public Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var textParameter = requestData.Form.GetValue<string>("name");
            var text = requestData.Form.GetValue<string>(textParameter);
            var model = new CodeWindowModel { Text = text, Name = textParameter };

            try
            {
                var jsonDocument = JsonDocument.Parse(text);
                model.Text = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
            }
            catch
            {
            }

            return componentFactory.GetPlainComponent(model,
                configureResponse: new HxHeaderBuilder()
                    .ReSwap("innerHTML")
                    .BuildResponseMutator());
        }
    }
}
