using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Admin.SpendLess.Admin;

namespace SpendLess.Admin.Controllers
{
    [Route("[controller]")]
    public class AdminController(IPageComponentFactory pageFactory) : BaseController
    {
        public async Task<IActionResult> Get()
        {
            var component = await pageFactory.GetComponent<AdminModel>();
            return component.CreateView(this);
        }
    }
}
