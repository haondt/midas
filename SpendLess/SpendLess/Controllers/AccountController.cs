using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Controllers
{
    [Route("[controller]")]
    public class AccountController(IPageComponentFactory pageFactory) : BaseController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(string id)
        {
            var component = await pageFactory.GetComponent<UpsertAccountModel>(new Dictionary<string, string>
            {
                { "id", id }
            });
            return component.CreateView(this);
        }
    }
}
