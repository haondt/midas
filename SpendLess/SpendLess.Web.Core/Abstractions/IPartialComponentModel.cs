using Haondt.Web.Core.Components;

namespace SpendLess.Web.Core.Abstractions
{
    public interface IPartialComponentModel : IComponentModel
    {
        string ViewPath { get; }
    }
}
