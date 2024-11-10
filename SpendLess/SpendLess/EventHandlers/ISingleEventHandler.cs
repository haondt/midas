using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace SpendLess.EventHandlers
{
    public interface ISingleEventHandler
    {
        public string EventName { get; }
        public Task<IComponent> HandleAsync(IRequestData requestData);

    }
}
