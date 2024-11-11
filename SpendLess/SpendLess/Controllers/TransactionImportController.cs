using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Controllers
{
    [Route("transaction-import")]
    public class TransactionImportController(IPageComponentFactory pageFactory) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var component = await pageFactory.GetComponent<TransactionImportModel>();
            return component.CreateView(this);
        }
    }
}
