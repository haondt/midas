using Haondt.Core.Models;
using Haondt.Web.Assets;
using Haondt.Web.Core.Reasons;
using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using SpendLess.Kvs.Services;
using System.Text;

namespace SpendLess.Admin.Services
{
    public class ExportMappingsAssetSource(IKvsService kvs) : IAssetSource
    {
        public async Task<Result<(byte[] Data, bool Cache), WebReason>> GetAssetAsync(string assetPath)
        {
            if (assetPath != "admin.kvsmappings.json")
                return new(WebReason.NotFound);

            var mappings = await kvs.ExportMappings();
            var mappingsString = JsonConvert.SerializeObject(mappings, SpendLessConstants.ApiSerializerSettings)
                ?? throw new NullReferenceException("Failed to serialize mappings");

            var bytes = Encoding.UTF8.GetBytes(mappingsString);
            return new((bytes, false));
        }
    }
}
