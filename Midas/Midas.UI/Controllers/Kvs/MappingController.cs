using Haondt.Core.Extensions;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Models;
using Midas.Domain.Kvs.Services;
using Midas.UI.Components.Kvs;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using Midas.UI.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Midas.Kvs.Controllers
{

    [Route("kvs/mapping")]
    public class MappingController(IComponentFactory componentFactory,
        IKvsService kvs) : MidasUIController
    {
        private const string ALIAS_MOVED_HEADER = "Midas-Alias-Moved";

        [HttpGet("{encodedKey}")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get(string encodedKey)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            var mapping = await kvs.GetMapping(key);

            var kvsComponent = new Midas.UI.Components.Kvs.Kvs
            {
                Mapping = new Mapping
                {
                    Key = mapping.Key,
                    EncodedKey = encodedKey,
                    Value = mapping.Value,
                    Aliases = mapping.Aliases,
                    IsSwap = false
                }
            };
            var x = Request.Headers.TryGetValue<bool>(ALIAS_MOVED_HEADER).Or(false);

            Microsoft.AspNetCore.Components.IComponent result = Request.Headers.TryGetValue<bool>(ALIAS_MOVED_HEADER).Or(false)
                ? new AppendComponentLayout
                {
                    Components = new()
                    {
                        kvsComponent,
                        new Toast
                        {
                            Message = "Alias moved.",
                            Severity = ToastSeverity.Success
                        }
                    }
                } : kvsComponent;

            return await componentFactory.RenderComponentAsync(kvsComponent);
        }

        [HttpGet]
        public async Task<IResult> GetFromQuery(
            [FromQuery]
            [Required(AllowEmptyStrings = false)]
            string key)
        {
            var mapping = await kvs.GetMapping(key);
            var encodedKey = StringFormatter.UrlBase64Encode(key);

            Response.AsResponseData()
                .HxPushUrl($"/kvs/mapping/{encodedKey}");
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new Mapping
                    {
                        EncodedKey = encodedKey,
                        Key = mapping.Key,
                        Value = mapping.Value,
                        Aliases = mapping.Aliases
                    },
                    new CloseModal()
                ]
            });
        }

        [HttpPost("{encodedKey}")]
        public async Task<IResult> UpsertMapping(
            string encodedKey,
            [FromForm]
            [Required(AllowEmptyStrings = false)]
            string value)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            await kvs.UpsertValue(key, value);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = $"Updated value for {key}.",
                Severity = ToastSeverity.Success
            });
        }

        [HttpDelete("{encodedKey}")]
        public async Task<IResult> DeleteMapping(
            string encodedKey)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            await kvs.DeleteMapping(key);

            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new List<Microsoft.AspNetCore.Components.IComponent>
                {
                    new Toast
                    {
                        Message = $"Deleted mapping {key}.",
                        Severity = ToastSeverity.Success
                    },
                    new Midas.UI.Components.Kvs.Kvs()
                }
            });
        }

        [HttpPatch("{encodedKey}/aliases")]
        public async Task<IResult> AddAlias(
            string encodedKey,
            [FromForm]
            [Required(AllowEmptyStrings = false)]
            string alias)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            var aliases = await kvs.AddAlias(key, alias);
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new Toast
                    {
                        Message = "Alias added!",
                        Severity = ToastSeverity.Success
                    },
                    new AliasList {
                        Aliases = aliases,
                        EncodedKey = encodedKey
                    }
                ]
            });
        }

        [HttpDelete("{encodedKey}/aliases")]
        public async Task<IResult> DeleteAlias(
            string encodedKey,
            [FromQuery]
            [Required]
            string alias)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            var aliases = await kvs.DeleteAlias(alias);
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new Toast
                    {
                        Message = "Alias removed.",
                        Severity = ToastSeverity.Success
                    },
                    new AliasList {
                        Aliases = aliases,
                        EncodedKey = encodedKey
                    }
                ]
            });
        }

        [HttpPost("{encodedKey}/move")]
        public async Task<IResult> MoveMapping(
            string encodedKey,
            [FromForm(Name = "new-key")]
            [Required(AllowEmptyStrings = false)]
            string newKey)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            await kvs.MoveMapping(key, newKey);
            var newEncodedKey = StringFormatter.UrlBase64Encode(newKey);

            var component = await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new Toast
                    {
                        Message = "Alias moved.",
                        Severity = ToastSeverity.Success
                    }
                ]
            });

            Response.AsResponseData()
                .HxTrigger("toastRelay",
                    new Dictionary<string, string>() { { "message", "Alias moved." }, { "severity", ToastSeverity.Success.ToString() } },
                    "#toast-relay")
                .HxLocation($"/kvs/mapping/{newEncodedKey}",
                    target: "#kvs-mapping",
                    select: "#kvs-mapping",
                    headers: new() { { ALIAS_MOVED_HEADER, "true" } });
            return component;
        }
    }
}
