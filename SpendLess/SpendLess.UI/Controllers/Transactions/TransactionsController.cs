using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Accounts.Services;
using SpendLess.Domain.Transactions.Services;
using SpendLess.UI.Components.Transactions;
using SpendLess.UI.Models.Transactions;
using SpendLess.UI.Services.Transactions;
using SpendLess.UI.Shared.Components;
using SpendLess.UI.Shared.Controllers;
using SpendLess.UI.Shared.ModelBinders;
using System.Text.RegularExpressions;

namespace SpendLess.UI.Controllers.Transactions
{
    [Route("transactions")]
    public class TransactionsController(IComponentFactory componentFactory,
        ITransactionService transactionService,
        ITransactionFilterService transactionFilterService,
        IAccountsService accountsService) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<SpendLess.UI.Components.Transactions.Transactions>();
        }

        [HttpDelete]
        public async Task<IResult> Delete(
            [FromQuery(Name = "page-size")] int? pageSize,
            [FromQuery] IEnumerable<string> filters,
            [FromQuery(Name = "select-all"), ModelBinder(typeof(CheckboxModelBinder))] bool selectAll)
        {
            if (selectAll)
            {
                var transactionFilters = (await transactionFilterService.ParseFiltersAsync(filters)).ToList();
                await transactionService.DeleteTransactions(transactionFilters);
            }
            else
            {
                var requestData = Request.AsRequestData();
                var targets = requestData.Query
                    .Where(kvp => Regex.IsMatch(kvp.Key, "^t-[0-9]+$"))
                    .Select(kvp => kvp.Key.Substring(2))
                    .Select(s => long.Parse(s));
                await transactionService.DeleteTransactions(targets.ToList());
            }

            var searchResult = await GetSearchResultComponent(filters, null, pageSize, null);
            Response.AsResponseData()
                .HxReswap("innerHTML")
                .HxRetarget("#search-results");
            return await componentFactory.RenderComponentAsync(searchResult);
        }


        [HttpPost("search/filter")]
        public Task<IResult> AddFilter([FromForm] string target)
        {
            var filterText = GetFilterText(target, Request.Form);
            if (filterText.HasValue)
                return componentFactory.RenderComponentAsync(new TransactionFilterComponent { Text = filterText.Value });

            throw new UserException($"Filter configuration not supported.");
        }

        [HttpGet("search/complete/{target}")]
        public async Task<IResult> SearchAutocomplete(string target)
        {
            switch (target)
            {
                case "source-account":
                case "destination-account":
                case "either-account":
                    {
                        var prefix = target switch
                        {
                            "source-account" => TransactionFilterTargets.SourceAccountName,
                            "destination-account" => TransactionFilterTargets.DestinationAccountName,
                            "either-account" => TransactionFilterTargets.EitherAccountName,
                            _ => throw new InvalidOperationException($"Unexpected target: {target}") // to make the compiler happy
                        };
                        var partialText = Request.Query.GetValueOrDefault($"{prefix}-value", "");

                        var accounts = await accountsService.SearchAccountsByName(partialText);
                        return await componentFactory.RenderComponentAsync(new AutocompleteSuggestions
                        {
                            Suggestions = accounts.Select(a => a.Name).ToList()
                        });
                    }
            }

            throw new InvalidOperationException($"Unknown autocomplete target: {target}");
        }

        private Optional<string> GetFilterText(string target, IEnumerable<KeyValuePair<string, StringValues>> requestPayload)
        {
            var value = requestPayload
                .TryGetValue<string>($"{target}-value");
            var op = requestPayload
                .GetValue<string>("operator");

            Optional<string> defaultedValue = target switch
            {
                TransactionFilterTargets.Amount => !value.HasValue
                    ? new Optional<string>()
                    : decimal.TryParse(value.Value, out var decimalValue)
                        ? new(decimalValue.ToString("F2"))
                        : new(),
                TransactionFilterTargets.Description
                    or TransactionFilterTargets.Category
                    or TransactionFilterTargets.Tags
                    or TransactionFilterTargets.SourceAccountName
                    or TransactionFilterTargets.DestinationAccountName
                    or TransactionFilterTargets.EitherAccountName
                    => value.Or(""),
                _ => new()
            };

            return defaultedValue.As(v => $"{target} {op} {v}");
        }

        [HttpPost("search")]
        public async Task<IResult> Search([FromForm] IEnumerable<string> filters,
            [FromForm(Name = "total-pages")] int? totalPages,
            [FromForm(Name = "page-size")] int? pageSize,
            [FromForm] int? page)
        {
            return await componentFactory.RenderComponentAsync(await GetSearchResultComponent(
                filters, totalPages, pageSize, page));
        }


        public async Task<SearchResult> GetSearchResultComponent([FromForm] IEnumerable<string> filters,
            [FromForm(Name = "total-pages")] long? totalPages,
            [FromForm(Name = "page-size")] long? pageSize,
            [FromForm] long? page)
        {
            pageSize ??= 25;
            page ??= 1;

            var transactionFilters = (await transactionFilterService.ParseFiltersAsync(filters)).ToList();
            var transactions = await transactionService.GetPagedExtendedTransactions(accountsService, transactionFilters, pageSize.Value, page.Value);
            if (!totalPages.HasValue)
            {
                var totalTransactions = await transactionService.GetTransactionsCount(transactionFilters);
                totalPages = (totalTransactions - 1) / pageSize.Value + 1;
            }

            return new SearchResult
            {
                Results = transactions,
                Page = page.Value,
                TotalPages = totalPages.Value,
                PageSize = pageSize.Value,
            };

        }

    }
}
