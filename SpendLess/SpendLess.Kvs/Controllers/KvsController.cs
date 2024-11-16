using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Kvs.SpendLess.Kvs;

namespace SpendLess.Controllers
{
    [Route("[controller]")]
    public class KvsController(IPageComponentFactory pageFactory) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var component = await pageFactory.GetComponent<KvsModel>();
            return component.CreateView(this);
        }

        [HttpGet("{mapping}")]
        public async Task<IActionResult> GetMapping(string mapping)
        {
            var component = await pageFactory.GetComponent<KvsModel>(new Dictionary<string, string>
            {
                { "mapping", mapping }
            });
            return component.CreateView(this);
        }
    }
}
