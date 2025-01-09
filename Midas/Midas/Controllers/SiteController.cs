using Microsoft.AspNetCore.Mvc;

namespace Midas.Controllers
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
