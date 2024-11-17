using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Core.Exceptions;
using SpendLess.Kvs.Models;
using SpendLess.Kvs.Services;
using SpendLess.Web.Domain.Extensions;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.Admin.EventHandlers
{
    public class ImportMappingsEventHandler(IComponentFactory componentFactory, IKvsService kvs) : ISingleEventHandler
    {
        public string EventName => "AdminImportMappings";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var file = (requestData.Form.Files?.FirstOrDefault(f => f.Name == "file"))
                ?? throw new UserException("Please select a file to import.");

            var overwriteExisting = requestData.Form.TryGetValue<string>("overwrite-existing").HasValue;
            var parsedData = file.DeserializeFromJson<ExternalKvsMappingsDto>();

            await kvs.ImportKvsMappings(parsedData, overwriteExisting);

            return await componentFactory.GetPlainComponent(new ToastModel
            {
                Message = "Imported mappings successfully!",
                Severity = ToastSeverity.Success,
            });
        }
    }
}
