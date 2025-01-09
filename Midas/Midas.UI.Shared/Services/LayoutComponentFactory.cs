using Haondt.Web.Services;
using Microsoft.AspNetCore.Components;
using Midas.UI.Shared.Components;

namespace Midas.UI.Shared.Services
{
    public class LayoutComponentFactory : ILayoutComponentFactory
    {
        public Task<IComponent> GetLayoutAsync(IComponent content)
        {
            return Task.FromResult<IComponent>(new Layout
            {
                Content = content
            });
        }
    }
}
