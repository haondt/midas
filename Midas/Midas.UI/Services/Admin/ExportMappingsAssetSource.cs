using Haondt.Core.Models;
using Haondt.Web.Assets;
using Haondt.Web.Core.Reasons;
using Midas.Core.Constants;
using Midas.Domain.Kvs.Services;
using Newtonsoft.Json;
using System.Text;

namespace Midas.UI.Services.Admin
{
    public class ExportMappingsAssetSource(IKvsService kvs) : IAssetSource
    {
        public async Task<DetailedResult<(byte[] Data, bool Cache), WebReason>> GetAssetAsync(string assetPath)
        {
            if (assetPath != "admin.kvsmappings.json")
                return new(WebReason.NotFound);

            var mappings = await kvs.ExportMappings();
            var mappingsString = JsonConvert.SerializeObject(mappings, MidasConstants.ApiSerializerSettings)
                ?? throw new NullReferenceException("Failed to serialize mappings");

            var bytes = Encoding.UTF8.GetBytes(mappingsString);
            return new((bytes, false));
        }
    }
}
