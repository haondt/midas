using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.NodeRed.Models;

namespace SpendLess.EventHandlers.NodeRed
{
    public class LoadDefaultNodeRedPayloadEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "NodeRedLoadDefault";

        public Task<IComponent> HandleAsync(IRequestData requestData)
        {
            return componentFactory.GetPlainComponent(new NodeRedUpdateModel
            {
                RequestText = new(new SendToNodeRedRequestDto().ToString())
            });
        }
    }
}
