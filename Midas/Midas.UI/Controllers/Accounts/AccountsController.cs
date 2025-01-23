using Haondt.Core.Extensions;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Exceptions;
using Midas.Core.Extensions;
using Midas.Domain.Accounts.Services;
using Midas.Persistence.Models;
using Midas.UI.Components.Accounts;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using Midas.UI.Shared.ModelBinders;
using Midas.UI.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Midas.Accounts.Controllers
{
    [Route("accounts")]
    public class AccountsController(IComponentFactory componentFactory,
        IAccountsService accountsService) : MidasUIController
    {
        [ServiceFilter(typeof(RenderPageFilter))]
        [HttpGet("all")]
        public Task<IResult> GetAllAccounts([FromQuery(Name = "partial-account-name")] string? partialAccountName)
        {
            var requestData = Request.AsRequestData();
            var pageSize = requestData.Query.TryGetValue<int>("page-size");
            var page = requestData.Query.TryGetValue<int>("page");
            var totalPages = requestData.Query.TryGetValue<int>("total-pages");
            return componentFactory.RenderComponentAsync(new AllAccounts
            {
                PageSize = pageSize.Test(out var q) ? q : null,
                Page = page.Or(1),
                TotalPages = totalPages.Test(out var q2) ? q2 : null,
                PartialAccountName = partialAccountName
            });
        }

        [ServiceFilter(typeof(RenderPageFilter))]
        [HttpGet("mine")]
        public Task<IResult> GetAccountsInNetWorth()
        {
            return componentFactory.RenderComponentAsync<AccountsInNetWorth>();
        }

        [HttpGet("create")]
        public Task<IResult> GetCreateModal([FromQuery(Name = "include-in-net-worth")] bool include)
        {
            return componentFactory.RenderComponentAsync(new CreateAccountModal { IncludeInNetWorth = include });
        }

        [HttpPost("create")]
        public async Task<IResult> Create(
            [FromForm, Required(AllowEmptyStrings = false)]
            string name,
            [FromForm(Name = "include-in-net-worth"), ModelBinder(typeof(CheckboxModelBinder))] bool includeInNetWorth)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new UserException("Account name cannot be empty");

            var accountId = Guid.NewGuid().ToString();
            await accountsService.UpsertAccount(accountId, new AccountDto
            {
                Name = name,
                IsMine = includeInNetWorth
            });

            Response.AsResponseData()
                .HxPushUrl($"/account/{accountId}");
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new Account {
                        Name = name,
                        Id = accountId,
                        Balance = 0,
                        IsIncludedInNetWorth = includeInNetWorth
                    },
                    new CloseModal(),
                    new Toast {
                        Message = "Account created successfully!",
                        Severity = ToastSeverity.Success
                    }]
            });
        }
    }
}
