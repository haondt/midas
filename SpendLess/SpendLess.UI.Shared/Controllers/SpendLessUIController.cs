using Microsoft.AspNetCore.Mvc;
using SpendLess.UI.Shared.Middlewares;

namespace SpendLess.UI.Shared.Controllers
{
    [Produces("text/html")]
    [ServiceFilter(typeof(ModelStateValidationFilter))]
    public class SpendLessUIController : Controller
    {
    }
}
