using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Domain.SpendLess.Domain
{
    public class TextModel : IComponentModel
    {
        public required string Text { get; set; }
    }

    public class TextComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<TextModel>((IComponentFactory cf, IRequestData rd) => new TextModel
            {
                Text = rd.Query.GetValue<string>("text")

            })
            {
                ViewPath = "~/SpendLess.Domain/Text.cshtml"
            };
        }
    }
}
