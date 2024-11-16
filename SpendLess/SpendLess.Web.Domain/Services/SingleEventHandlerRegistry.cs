using Haondt.Core.Models;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Haondt.Web.Services;

namespace SpendLess.Web.Domain.Services
{
    public class SingleEventHandlerRegistry(
        IEnumerable<ISingleEventHandler> singleEventHandlers,
        IComponentFactory componentFactory) : IEventHandler
    {
        Dictionary<string, List<ISingleEventHandler>> _singleEventHandlers = singleEventHandlers.GroupBy(h => h.EventName)
            .ToDictionary(grp => grp.Key, grp => grp.ToList());

        public async Task<Optional<IComponent>> HandleAsync(string eventName, IRequestData requestData)
        {
            if (!_singleEventHandlers.TryGetValue(eventName, out var handlers))
                return new();

            var componentTasks =
                handlers.Select(async h => await h.HandleAsync(requestData));
            var components = (await Task.WhenAll(componentTasks)).ToList();

            if (components.Count == 1)
                return new(components[0]);

            var appendLayout = await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = components
            });
            return new(appendLayout);
        }
    }
}
