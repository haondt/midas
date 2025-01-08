using Haondt.Core.Models;
using Haondt.Web.Assets;
using Haondt.Web.Core.Reasons;
using SpendLess.Domain.Admin.Models;
using SpendLess.Domain.Admin.Services;
using SpendLess.Domain.Shared.Services;

namespace SpendLess.UI.Services.Admin
{
    public class TakeoutAssetSource(IAsyncJobRegistry jobRegistry, IFileService fileService) : IAssetSource
    {
        public Task<Result<(byte[] Data, bool Cache), WebReason>> GetAssetAsync(string assetPath)
        {
            var prefix = "admin.takeout.";
            var postfix = ".zip";
            if (!assetPath.StartsWith(prefix) || !assetPath.EndsWith(postfix))
                return Task.FromResult<Result<(byte[] Data, bool Cache), WebReason>>(new(WebReason.NotFound));

            var jobId = assetPath.Substring(prefix.Length, assetPath.Length - prefix.Length - postfix.Length);
            var result = jobRegistry.GetJobResult(jobId);
            if (!result.HasValue
                || result.Value is not TakeoutResult takeout
                || !takeout.IsSuccessful
                || !takeout.ZipPath.HasValue)
                return Task.FromResult<Result<(byte[] Data, bool Cache), WebReason>>(new(WebReason.NotFound));

            var bytes = fileService.ReadBytes(takeout.ZipPath.Value);
            return Task.FromResult<Result<(byte[] Data, bool Cache), WebReason>>(new((bytes, true)));
        }
    }
}
