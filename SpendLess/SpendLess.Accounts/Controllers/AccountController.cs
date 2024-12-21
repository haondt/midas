using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Accounts.Components;
using SpendLess.Accounts.Models;
using SpendLess.Accounts.Services;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Models;
using SpendLess.Web.Core.Exceptions;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Accounts.Controllers
{
    [Route("account")]
    public class AccountController(IComponentFactory componentFactory,
        IAccountsService accountsService) : SpendLessUIController
    {
        [HttpGet("{id}")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> GetAccount(string id)
        {
            var account = await accountsService.TryGetOwnedAccount(id);
            if (!account.HasValue)
                throw new NotFoundPageException($"Account {id} not found.");

            return await componentFactory.RenderComponentAsync(new Account
            {
                Id = id,
                Name = account.Value.Name
            });
        }

        [HttpDelete("{id}")]
        public async Task<IResult> DeleteAccount(string id)
        {
            await accountsService.Disown(id);
            return Results.NoContent();
        }

        [HttpPost("{id}")]
        public async Task<IResult> UpdateAccount(
            string id,
            [FromForm] UpsertAccountRequest request)
        {
            request.Name = request.Name.Trim();

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new UserException("Account name cannot be empty");

            await accountsService.UpsertOwnedAccount(id, new AccountDto
            {
                Name = request.Name
            });

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Updated account successfully.",
                Severity = Web.Domain.Models.ToastSeverity.Success
            });
        }
    }
}
