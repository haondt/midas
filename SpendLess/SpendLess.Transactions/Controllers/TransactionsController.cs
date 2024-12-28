using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Core.Exceptions;
using SpendLess.Transactions.Components;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Models.ViewModels;
using SpendLess.Transactions.Services;
using SpendLess.Web.Core.ModelBinders;
using SpendLess.Web.Domain.Controllers;
using System.Text.RegularExpressions;

namespace SpendLess.Transactions.Controllers
{
    [Route("transactions")]
    public class TransactionsController(IComponentFactory componentFactory,
        ITransactionService transactionService) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get()
        {
            return await componentFactory.RenderComponentAsync(new Transactions.Components.Transactions
            {
                Tags = await transactionService.GetTags(),
                Categories = await transactionService.GetCategories()
            });
        }

        [HttpDelete]
        public async Task<IResult> Delete(
            [FromQuery(Name = "page-size")] int? pageSize,
            [FromQuery] IEnumerable<string> filters,
            [FromQuery(Name = "select-all"), ModelBinder(typeof(CheckboxModelBinder))] bool selectAll)
        {
            if (selectAll)
            {
                var transactionFilters = ParseFilters(filters).ToList();
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
            var filterText = GetFilterText(target);
            if (filterText.HasValue)
                return componentFactory.RenderComponentAsync(new TransactionFilterComponent { Text = filterText.Value });

            throw new UserException($"Filter configuration not supported.");
        }

        private Optional<string> GetFilterText(string target)
        {
            var rd = Request.AsRequestData();
            var value = rd.Form
                .TryGetValue<string>($"{target}-value");
            var op = rd.Form
                .GetValue<string>("operator");

            Optional<string> defaultedValue = target switch
            {
                TransactionFilterTargets.Amount => !value.HasValue
                    ? new Optional<string>()
                    : decimal.TryParse(value.Value, out var decimalValue)
                        ? new(decimalValue.ToString("F2"))
                        : new(),
                TransactionFilterTargets.Description => value.Or(""),
                TransactionFilterTargets.Category => value.Or(""),
                TransactionFilterTargets.Tags => value.Or(""),
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

        private IEnumerable<TransactionFilter> ParseFilters(IEnumerable<string> filters)
        {
            return filters.Select(filter =>
            {
                var splitFilter = filter.Split(' ');
                var target = splitFilter[0];
                var op = splitFilter[1];
                var value = string.Join(' ', splitFilter[2..]);

                switch (target)
                {
                    case TransactionFilterTargets.Description:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.DescriptionContains(value);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Category:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.HasCategory(target);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Amount:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsGreaterThanOrEqualTo:
                                return TransactionFilter.MinAmount(decimal.Parse(value));
                            case TransactionFilterOperators.IsLessThanOrEqualTo:
                                return TransactionFilter.MaxAmount(decimal.Parse(value));
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.HasAmount(decimal.Parse(value));
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Tags:
                        switch (op)
                        {
                            case TransactionFilterOperators.Contains:
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.HasTag(value);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    default:
                        throw new NotSupportedException($"Target {target} is not supported.");
                }
            }).ToList();
        }

        public async Task<SearchResult> GetSearchResultComponent([FromForm] IEnumerable<string> filters,
            [FromForm(Name = "total-pages")] long? totalPages,
            [FromForm(Name = "page-size")] long? pageSize,
            [FromForm] long? page)
        {
            pageSize ??= 25;
            page ??= 1;

            var transactionFilters = ParseFilters(filters).ToList();
            var transactions = await transactionService.GetPagedExtendedTransactions(transactionFilters, pageSize.Value, page.Value);
            if (!totalPages.HasValue)
            {
                var totalTransactions = await transactionService.GetTransactionsCount(transactionFilters);
                totalPages = ((totalTransactions - 1) / pageSize.Value) + 1;
            }

            return new SearchResult
            {
                Results = transactions,
                Page = page.Value,
                TotalPages = totalPages.Value
            };

        }

    }
}
