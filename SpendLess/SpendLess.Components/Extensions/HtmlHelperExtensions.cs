using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpendLess.Components.Abstractions;
using SpendLess.Components.Extensions;

namespace SpendLess.Components.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static Task<IHtmlContent> PartialAsync(this IHtmlHelper html, IPartialComponentModel partialModel)
        {
            return html.PartialAsync(partialModel.ViewPath, partialModel);
        }
    }
}
