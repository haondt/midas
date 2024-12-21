using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Accounts.Components;
using SpendLess.Accounts.Services;
using SpendLess.Domain.Models;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.Accounts.Controllers
{
    [Route("accounts")]
    public class AccountsController(IComponentFactory componentFactory,
        IAccountsService accountsService) : SpendLessUIController
    {
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<SpendLess.Accounts.Components.Accounts>();
        }

        [HttpGet("create")]
        public Task<IResult> GetCreateModal()
        {
            return componentFactory.RenderComponentAsync<CreateAccountModal>();
        }

        [HttpPost("create")]
        public async Task<IResult> Create(
            [FromForm, Required(AllowEmptyStrings = false)]
            string name)
        {
            name = name.Trim();
            var accountId = Guid.NewGuid().ToString();
            await accountsService.UpsertOwnedAccount(accountId, new AccountDto
            {
                Name = name
            });


            Response.AsResponseData()
                .HxPushUrl($"/accounts/{accountId}");
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new Account {
                        Name = name,
                        Id = accountId
                    },
                    new CloseModal(),
                    new Toast {
                        Message = "Account created successfully!",
                        Severity = Web.Domain.Models.ToastSeverity.Success
                    }]
            });
        }
    }
}
