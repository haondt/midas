using Haondt.Web.Core.Components;

namespace SpendLess.Components.Abstractions
{
    public interface IPartialComponentModel : IComponentModel
    {
        string ViewPath { get; }
    }
}
