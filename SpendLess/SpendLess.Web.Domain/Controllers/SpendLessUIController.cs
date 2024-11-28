using Microsoft.AspNetCore.Mvc;
using SpendLess.Web.Domain.Middlewares;

namespace SpendLess.Web.Domain.Controllers
{
    [Produces("text/html")]
    [ServiceFilter(typeof(ModelStateValidationFilter))]
    public class SpendLessUIController : Controller
    {
    }
}
