using Haondt.Core.Models;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Transactions.Services;
using Midas.UI.Components.TransactionsSelect;
using Midas.UI.Services.Transactions;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using Midas.UI.Shared.ModelBinders;
using System.Text.RegularExpressions;

namespace Midas.UI.Controllers.TransactionsSelect
{
    [Route("transactions-select")]
    public class TransactionsSelectController(IComponentFactory componentFactory,
        ITransactionFilterService transactionFilterService,
        ITransactionService transactionService) : MidasUIController
    {
        [HttpGet("search")]
        public async Task<IResult> GetSearchModal(
            [FromQuery(Name = "selection-event")] string? selectionEvent)
        {
            return await componentFactory.RenderComponentAsync(new TransactionsSelectModal
            {
                SelectionEvent = string.IsNullOrEmpty(selectionEvent) ? new Optional<string>() : selectionEvent
            });
        }

        [HttpPost("search")]
        public async Task<IResult> FinishSearch(
            [FromForm(Name = "selection-event")] string? selectionEvent,
            [FromForm] IEnumerable<string> filters,
            [FromForm(Name = "select-all"), ModelBinder(typeof(CheckboxModelBinder))] bool selectAll)
        {
            var result = new TransactionsSelectField
            {
                Swap = true,
                SelectionEvent = string.IsNullOrEmpty(selectionEvent) ? new Optional<string>() : selectionEvent,
            };

            if (selectAll)
            {
                var parsedFilters = (await transactionFilterService.ParseFiltersAsync(filters)).ToList();
                result.SelectedTransactions = await transactionService.GetTransactionsCount(parsedFilters);
                result.SelectedTransactionFilters = filters.ToList();
            }
            else
            {
                result.SelectedTransactionIds = GetSelectedTransactionIds(Request.AsRequestData());
                result.SelectedTransactions = result.SelectedTransactionIds.Count;
            }

            if (result.SelectionEvent.HasValue)
                Response.Headers.Add("HX-Trigger-After-Swap", result.SelectionEvent.Value);
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    result,
                    new CloseModal()
                ]
            });
        }

        public static List<long> GetSelectedTransactionIds(IRequestData requestData)
        {
            return requestData.Form
                .Where(kvp => Regex.IsMatch(kvp.Key, "^t-[0-9]+$"))
                .Select(kvp => kvp.Key.Substring(2))
                .Select(s => long.Parse(s))
                .ToList();
        }
    }
}
