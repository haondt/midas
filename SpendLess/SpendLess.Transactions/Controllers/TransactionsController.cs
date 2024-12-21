using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Transactions.Components;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Models.ViewModels;
using SpendLess.Transactions.Services;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Transactions.Controllers
{
    [Route("transactions")]
    public class TransactionsController(IComponentFactory componentFactory,
        ITransactionService transactionService) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync(new Transactions.Components.Transactions
            {

            });
        }

        [HttpPost("search/filter")]
        public Task<IResult> AddFilter([FromForm] string target)
        {
            var filterText = GetFilterText(target);
            if (filterText.HasValue)
                return componentFactory.RenderComponentAsync(new TransactionFilterComponent { Text = filterText.Value });

            return Task.FromResult(Results.BadRequest());
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
            pageSize ??= 25;
            page ??= 1;

            var transactionFilters = filters.Select(filter =>
            {
                var splitFilter = filter.Split(' ');
                var target = splitFilter[0];
                var op = splitFilter[1];
                var value = splitFilter[2];

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
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    default:
                        throw new NotSupportedException($"Target {target} is not supported.");
                }
            }).ToList();

            var transactions = await transactionService.GetPagedExtendedTransactions(transactionFilters, pageSize.Value, page.Value);
            if (!totalPages.HasValue)
            {
                var totalTransactions = await transactionService.GetTransactionsCount(transactionFilters);
                totalPages = totalTransactions / pageSize.Value;
            }
            totalPages ??= await transactionService.GetTransactionsCount(transactionFilters);

            return await componentFactory.RenderComponentAsync(new SearchResult
            {
                Results = transactions,
                Page = page.Value,
                TotalPages = totalPages.Value
            });
        }

    }
}
