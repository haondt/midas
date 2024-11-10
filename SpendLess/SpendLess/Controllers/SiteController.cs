using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Controllers
{
    public class SiteController(IPageComponentFactory pageFactory) : BaseController
    {
        [HttpGet("/")]
        public IActionResult Get()
        {
            // TODO
            return Redirect("accounts");
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> Get(string page)
        {
            if (page == "accounts")
            {
                var component = await pageFactory.GetComponent<AccountsModel>();
                return component.CreateView(this);

            }

            return NotFound();
        }
    }
}
