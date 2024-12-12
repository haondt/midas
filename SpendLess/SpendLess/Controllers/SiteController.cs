using Microsoft.AspNetCore.Mvc;

namespace SpendLess.Controllers
{
    public class SiteController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Get()
        {
            return Redirect("dashboard");
        }
    }
}
