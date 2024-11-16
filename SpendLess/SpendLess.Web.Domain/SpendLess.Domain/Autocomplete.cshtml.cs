using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Web.Domain.SpendLess.Domain
{
    public class AutocompleteModel : IPartialComponentModel
    {
        public string ViewPath => AutocompleteComponentDescriptorFactory.ViewPath;
    }

    public class AutocompleteSuggestionsModel : AutocompleteModel
    {
        public List<string> Suggestions { get; set; } = [];
    }

    public class AutocompleteBodyModel : AutocompleteModel
    {
        public required string SuggestionEvent { get; set; }
        public Optional<string> Name { get; set; } = new();
        public Optional<string> Id { get; set; } = new();
        public Optional<string> Placeholder { get; set; } = new();
        public Optional<string> HxInclude { get; set; } = new();
        public Optional<string> CompletionEvent { get; set; } = new();
        public bool IsRight { get; set; } = false;

        public string HxIncludeString
        {
            get
            {
                var result = "previous .input";
                if (HxInclude.HasValue)
                    result = $"{HxInclude.Value}, {result}";
                return result;
            }
        }

    }

    public class AutocompleteComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public static string ViewPath = "~/SpendLess.Domain/Autocomplete.cshtml";
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<AutocompleteModel>
            {
                ViewPath = ViewPath
            };
        }
    }
}

