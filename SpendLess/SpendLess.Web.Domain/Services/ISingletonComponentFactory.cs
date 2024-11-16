using Haondt.Web.Core.Components;

namespace SpendLess.Components.Services
{
    public interface ISingletonComponentFactory
    {
        IComponentFactory CreateComponentFactory();
    }
}
