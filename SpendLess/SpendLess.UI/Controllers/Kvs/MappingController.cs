﻿using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Core.Models;
using SpendLess.Domain.Kvs.Services;
using SpendLess.UI.Components.Kvs;
using SpendLess.UI.Shared.Components;
using SpendLess.UI.Shared.Controllers;
using SpendLess.UI.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.Kvs.Controllers
{
    [Route("kvs/mapping")]
    public class MappingController(IComponentFactory componentFactory,
        IKvsService kvs) : SpendLessUIController
    {
        [HttpGet("{encodedKey}")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get(string encodedKey)
        {
            var key = StringFormatter.UrlBase64Decode(encodedKey);
            var mapping = await kvs.GetExpandedMapping(key);
            return await componentFactory.RenderComponentAsync(new SpendLess.UI.Components.Kvs.Kvs
            {
                Mapping = new Mapping
                {
                    Key = mapping.Key,
                    EncodedKey = encodedKey,
                    Value = mapping.Value,
                    Aliases = mapping.Aliases,
                    IsSwap = false
                }
            });
        }

        [HttpGet]
        public async Task<IResult> GetFromQuery(
            [FromQuery]
            [Required(AllowEmptyStrings = false)]
            string key)
        {
            var mapping = await kvs.GetExpandedMapping(key);
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
            var aliases = await kvs.RemoveAlias(key, alias);
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
                        EncodedKey = key
                    }
                ]
            });
        }
    }
}