using Haondt.Web.Core.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace SpendLess.Controllers
{
    public class SiteController : BaseController
    {
        [HttpGet("/")]
        public IActionResult Get()
        {
            return Redirect("accounts");
        }
    }
}
