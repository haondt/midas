using SpendLess.Kvs.Services;

namespace SpendLess.Kvs.SpendLess.Kvs
{
    public class KvsComponentDescriptorFactory(IKvsService kvs)
    {

        //private async Task<(KvsModel, Optional<Action<IHttpResponseMutator>>)> ComponentFactory(IComponentFactory componentFactory, IRequestData requestData)
        //{
        //    if (requestData.Query.TryGetValue<string>("mapping", out var mapping))
        //    {
        //        var expanded = await kvs.GetExpandedMapping(mapping);
        //        return (new KvsLayoutModel { Content = KvsContentModel.FromExpandedMappingDto(expanded) }, new());
        //    }

        //    if (requestData.Query.TryGetValue<bool>("launch-select-modal").Or(false) == true)
        //        return (new KvsModalModel(), new());

        //    if (requestData.Query.TryGetValue<string>("search", out var searchText))
        //    {
        //        searchText = searchText.Trim();
        //        if (string.IsNullOrWhiteSpace(searchText))
        //            throw new UserException("Key cannot be empty.");

        //        Func<string, Action<IHttpResponseMutator>> responseMutatorFactory = k => new HxHeaderBuilder()
        //            .PushUrl($"/kvs/{k}")
        //            .BuildResponseMutator();

        //        var expandedMapping = await kvs.GetExpandedMapping(searchText);
        //        var model = KvsInsertContentModel.FromExpandedMappingDto(expandedMapping);
        //        return (model, responseMutatorFactory(expandedMapping.Key));
        //    }

        //    return (new KvsLayoutModel(), new());
        //}
    }
}
