using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Core.Extensions;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Kvs.Services
{
    public class AutocompleteEventHandler(IComponentFactory componentFactory, IKvsService kvs) : ISingleEventHandler
    {
        public string EventName => "KvsAutocomplete";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var suggestions = await kvs.Search(requestData.Form.TryGetValue<string>("search").Or(""));
            return await componentFactory.GetPlainComponent<AutocompleteModel>(new AutocompleteSuggestionsModel
            {
                Suggestions = suggestions
            }, configureResponse: m => m.ConfigureHeadersAction = new HxHeaderBuilder()
                .ReSwap("innerHTML")
                .Build());
        }
    }
}
