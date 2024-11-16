using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Domain.SpendLess.Domain
{
    public class CodeWindowModel : IPartialComponentModel
    {
        public Optional<string> Title { get; set; } = new();
        public Optional<string> Name { get; set; } = new();
        public Optional<string> Id { get; set; } = new();
        public string Text { get; set; } = "";

        public string ViewPath => CodeWindowComponentDescriptorFactory.ViewPath;
    }

    public class CodeWindowComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public static string ViewPath = "~/SpendLess.Domain/CodeWindow.cshtml";
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<CodeWindowModel>(() => new())
            {
                ViewPath = ViewPath,
            };
        }
    }
}
