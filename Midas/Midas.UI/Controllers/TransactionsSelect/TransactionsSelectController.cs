using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Midas.UI.Components.TransactionsSelect;
using Midas.UI.Models.Transactions;
using Midas.UI.Models.TransactionsSelect;
using Midas.UI.Services.Transactions;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using System.ComponentModel.DataAnnotations;

namespace Midas.UI.Controllers.TransactionsSelect
{
    [Route("transactions-select")]
    public class TransactionsSelectController(IComponentFactory componentFactory,
        ITransactionFilterService transactionFilterService) : MidasUIController
    {
        [HttpGet("search")]
        public async Task<IResult> GetSearchModal(
            [FromQuery] TransactionSearchOptions options,
            [FromQuery] TransactionsSelectionState selection)
        {
            return await componentFactory.RenderComponentAsync(new TransactionsSelectModal
            {
                Options = options,
                Selection = selection
            });
        }

        [HttpGet("static-search")]
        public Task<IResult> GetStaticSearchModal(
            [FromQuery] TransactionSearchOptions options,
            [FromQuery(Name = "filter")] List<string> filters)
        {
            return componentFactory.RenderComponentAsync(new TransactionsSelectStaticModal
            {
                Filters = filters,
                Options = options
            });
        }

        [HttpGet("single-search")]
        public Task<IResult> GetSingleSearchModal()
        {
            return componentFactory.RenderComponentAsync<TransactionsSelectSingleModal>();
        }

        [HttpPost("single-select")]
        public Task<IResult> FinishSingleSearch([FromForm(Name = "transaction"), Required] long transactionId)
        {
            Response.AsResponseData()
                .HxTrigger(
                    @event: TransactionsSelectSingleField.SelectionEvent,
                    target: TransactionsSelectSingleField.EventTarget,
                    body: transactionId.ToString()
                );
            return componentFactory.RenderComponentAsync<CloseModal>();
        }

        [HttpPost("search")]
        public async Task<IResult> FinishSearch(
            [FromForm] TransactionSelectionResult selection)
        {
            var result = new TransactionsSelectField
            {
                Swap = true,
                Selection = selection.GetSelectionState(Request.AsRequestData().Form, transactionFilterService)
            };

            if (selection.SelectionEvent != null)
                Response.Headers.Add("HX-Trigger-After-Swap", selection.SelectionEvent);
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    result,
                    new CloseModal()
                ]
            });
        }

    }
}
