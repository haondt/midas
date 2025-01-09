using Microsoft.AspNetCore.Mvc;
using Midas.UI.Shared.Middlewares;

namespace Midas.UI.Shared.Controllers
{
    [Produces("text/html")]
    [ServiceFilter(typeof(ModelStateValidationFilter))]
    public class MidasUIController : Controller
    {
    }
}
