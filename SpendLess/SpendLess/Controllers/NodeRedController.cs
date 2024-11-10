using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Controllers
{
    [Route("node-red")]
    public class NodeRedController(IPageComponentFactory pageFactory) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var component = await pageFactory.GetComponent<NodeRedModel>();
            return component.CreateView(this);
        }
    }
}
