using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Accounts.Components;
using SpendLess.Accounts.Services;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Models;
using SpendLess.Web.Core.ModelBinders;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using System.ComponentModel.DataAnnotations;

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
            var account = await accountsService.GetDetails(id);

            return await componentFactory.RenderComponentAsync(new Account
            {
                Id = id,
                Name = account.Name,
                Balance = account.Balance,
                IsIncludedInNetWorth = account.IsIncludedInNetWorth,
            });
        }

        [HttpDelete("{id}")]
        public async Task<IResult> DeleteAccount(string id)
        {
            await accountsService.DeleteAccount(id);
            return Results.NoContent();
        }



        [HttpPost("{id}")]
        public async Task<IResult> UpdateAccount(
            string id,
            [FromForm, Required(AllowEmptyStrings = false)] string name,
            [FromForm(Name = "include-in-net-worth"), ModelBinder(typeof(CheckboxModelBinder))] bool includeInNetWorth)
        {
            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new UserException("Account name cannot be empty");

            await accountsService.UpsertAccount(id, new AccountDto
            {
                Name = name
            }, includeInNetWorth);

            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Updated account successfully.",
                Severity = Web.Domain.Models.ToastSeverity.Success
            });
        }
    }
}
