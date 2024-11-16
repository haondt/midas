﻿using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Extensions;
using SpendLess.NodeRed.Services;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;
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

            var (status, result) = await nodeRed.SendToNodeRedRaw(body.Value);

            if (result.HasValue)
            {

                try
                {
                    var jsonDocument = JsonDocument.Parse(result.Value);
                    result = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                    });
                }
                catch
                {
                }
            }

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = new List<IComponent>
                {
                    await componentFactory.GetPlainComponent(new NodeRedUpdateModel
                    {
                        ResponseText = result.Or(""),
                        ResponseStatus = status
                    }),
                    await componentFactory.GetPlainComponent(new ToastModel
                    {
                        Message =  "Sent to Node Red successfully.",
                        Severity = ToastSeverity.Success
                    })
                }
            });
        }
    }
}
