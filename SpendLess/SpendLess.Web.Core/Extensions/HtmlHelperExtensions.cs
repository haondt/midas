using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpendLess.Web.Core.Abstractions;
using SpendLess.Web.Core.Extensions;

namespace SpendLess.Web.Core.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static Task<IHtmlContent> PartialAsync(this IHtmlHelper html, IPartialComponentModel partialModel)
        {
            return html.PartialAsync(partialModel.ViewPath, partialModel);
        }
    }
}
