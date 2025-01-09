﻿using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Domain.Kvs.Services;
using SpendLess.UI.Components.Kvs;
using SpendLess.UI.Shared.Components;
using SpendLess.UI.Shared.Controllers;

namespace SpendLess.Kvs.Controllers
{
    [Route("kvs/mappings")]
    public class MappingsController(IComponentFactory componentFactory,
        IKvsService kvs) : SpendLessUIController
    {

        [HttpGet]
        public Task<IResult> LaunchMappingsModal()
        {
            return componentFactory.RenderComponentAsync<SelectMappingModal>();
        }

        [HttpGet("search")]
        public async Task<IResult> SearchMapping([FromQuery] string? query)
        {
            var suggestions = await kvs.Search(query ?? "");
            return await componentFactory.RenderComponentAsync(new AutocompleteSuggestions
            {
                Suggestions = suggestions
            });
        }
    }
}