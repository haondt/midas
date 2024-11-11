using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Controllers
{
    [Route("[controller]")]
    public class AccountsController(IPageComponentFactory pageFactory) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var component = await pageFactory.GetComponent<AccountsModel>();
            return component.CreateView(this);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var component = await pageFactory.GetComponent<UpsertAccountModel>();
            return component.CreateView(this);
        }
    }
}
