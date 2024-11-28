using Haondt.Web.Services;
using Microsoft.AspNetCore.Components;
using SpendLess.Web.Domain.Components;

namespace SpendLess.Web.Domain.Services
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
