namespace SpendLess.Kvs.Services
{
    //public class AutocompleteEventHandler(IComponentFactory componentFactory, IKvsService kvs) : ISingleEventHandler
    //{
    //    public string EventName => "KvsAutocomplete";

    //    public async Task<IComponent> HandleAsync(IRequestData requestData)
    //    {
    //        var suggestions = await kvs.Search(requestData.Form.TryGetValue<string>("search").Or(""));
    //        return await componentFactory.GetPlainComponent<AutocompleteModel>(new AutocompleteSuggestionsModel
    //        {
    //            Suggestions = suggestions
    //        }, configureResponse: m => m.ConfigureHeadersAction = new HxHeaderBuilder()
    //            .ReSwap("innerHTML")
    //            .Build());
    //    }
    //}
}
