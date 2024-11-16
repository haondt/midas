using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Kvs.Services
{
    public class AutocompleteEventHandler(IComponentFactory componentFactory) : ISingleEventHandler
    {
        public string EventName => "KvsAutocomplete";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            return await componentFactory.GetPlainComponent<AutocompleteModel>(new AutocompleteSuggestionsModel
            {
                Suggestions = new List<string>
                {
                    "beans",
                    "toast"
                }
            }, configureResponse: m => m.ConfigureHeadersAction = new HxHeaderBuilder()
                .ReSwap("innerHTML")
                .Build());
        }
    }
}
