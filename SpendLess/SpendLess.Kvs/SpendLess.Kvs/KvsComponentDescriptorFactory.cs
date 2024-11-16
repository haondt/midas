using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Core.Exceptions;
using SpendLess.Core.Extensions;
using SpendLess.Kvs.Services;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Kvs.SpendLess.Kvs
{
    public class KvsComponentDescriptorFactory(IKvsService kvs) : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<KvsModel>(ComponentFactory)
            {
                ViewPath = $"~/SpendLess.Kvs/Kvs.cshtml",
            };
        }

        private async Task<(KvsModel, Optional<Action<IHttpResponseMutator>>)> ComponentFactory(IComponentFactory componentFactory, IRequestData requestData)
        {
            if (requestData.Query.TryGetValue<string>("mapping", out var mapping))
            {
                var expanded = await kvs.GetExpandedMapping(mapping);
                return (new KvsModel
                {
                    EditMapping = EditMappingModel.FromExpandedMappingDto(expanded),
                    WithLayout = true,
                }, new());
            }

            if (requestData.Query.TryGetValue<bool>("launch-select-modal").Or(false) == true)
                return (new KvsModel() { LaunchSelectModal = true }, new());

            if (requestData.Query.TryGetValue<string>("search", out var searchText))
            {
                if (searchText == "")
                    throw new UserException("Key cannot be empty.");

                (KvsModel, Optional<Action<IHttpResponseMutator>>) constructResponse(EditMappingModel model) =>
                    (new KvsModel
                    {
                        CloseModal = true,
                        EditMapping = model
                    }, new(new HxHeaderBuilder()
                        .PushUrl($"/kvs/{model.Key}")
                        .BuildResponseMutator()));

                var key = (await kvs.Search(searchText)).Or(searchText);
                if ((await kvs.Search(searchText)).Test(out var value))
                {
                    var expandedMapping = await kvs.GetExpandedMapping(value);
                    return constructResponse(EditMappingModel.FromExpandedMappingDto(expandedMapping));
                }
                else
                {
                    return constructResponse(new EditMappingModel { Key = searchText });
                }
            }

            return (new KvsModel() { WithLayout = true }, new());
        }
    }
}
