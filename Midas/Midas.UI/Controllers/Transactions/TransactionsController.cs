using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Midas.Core.Constants;
using Midas.Core.Exceptions;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Transactions.Models;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.UI.Components.Transactions;
using Midas.UI.Models.Transactions;
using Midas.UI.Services.Transactions;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;
using Midas.UI.Shared.Exceptions;
using Midas.UI.Shared.ModelBinders;
using System.Text.RegularExpressions;

namespace Midas.UI.Controllers.Transactions
{
    [Route("transactions")]
    public class TransactionsController(IComponentFactory componentFactory,
        ITransactionService transactionService,
        ITransactionFilterService transactionFilterService,
        IAccountsService accountsService) : MidasUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Midas.UI.Components.Transactions.Transactions>();
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

        [HttpGet("edit")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> GetCreatePage()
        {
            return componentFactory.RenderComponentAsync(new EditTransaction
            {

            });
        }

        [HttpGet("edit/{id}")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> GetEditPage(long id)
        {
            var transaction = await transactionService.GetExtendedTransaction(accountsService, id);
            if (!transaction.HasValue)
                throw new NotFoundPageException();
            return await componentFactory.RenderComponentAsync(EditTransaction.FromExtendedTransaction(id, transaction.Value));
        }

        [HttpPost("edit")]
        public async Task<IResult> CreateTransaction([FromForm] UpsertTransactionRequestDto requestDto)
        {
            var (newAccounts, newTransaction) = await ProcessUpsertTransactionRequest(requestDto);
            if (newAccounts.Count > 0)
                await accountsService.CreateAccounts(newAccounts);

            var transactionId = await transactionService.CreateTransaction(newTransaction);
            Response.AsResponseData()
                .HxPushUrl($"/transactions/edit/{transactionId}");
            return await componentFactory.RenderComponentAsync(EditTransaction.FromExtendedTransaction(transactionId, newTransaction));
        }

        [HttpPost("edit/{id}")]
        public async Task<IResult> UpdateTransaction([FromRoute] long id, [FromForm] UpsertTransactionRequestDto requestDto)
        {
            var (newAccounts, newTransaction) = await ProcessUpsertTransactionRequest(requestDto);
            if (newAccounts.Count > 0)
                await accountsService.CreateAccounts(newAccounts);

            var newIds = await transactionService.ReplaceTransactions([newTransaction], [id]);
            var transactionId = newIds[0];
            Response.AsResponseData()
                .HxPushUrl($"/transactions/edit/{transactionId}");
            return await componentFactory.RenderComponentAsync(EditTransaction.FromExtendedTransaction(transactionId, newTransaction));
        }

        private async Task<(List<(string Id, AccountDto Account)> NewAccounts, ExtendedTransactionDto NewTransaction)> ProcessUpsertTransactionRequest(UpsertTransactionRequestDto requestDto)
        {
            var newAccounts = new List<(string, AccountDto)>();

            if (requestDto.SourceAccount == MidasConstants.DefaultAccount)
                throw new UserException($"Source account id must be provided.");
            var account = await accountsService.TryGetAccount(requestDto.SourceAccount);
            if (account.HasValue)
                requestDto.SourceAccountName = account.Value.Name;
            else
            {
                requestDto.SourceAccountName ??= MidasConstants.FallbackAccountName;
                newAccounts.Add((requestDto.SourceAccount, new AccountDto
                {
                    IsMine = false,
                    Name = requestDto.SourceAccountName
                }));
            }

            if (requestDto.DestinationAccount == MidasConstants.DefaultAccount)
                throw new UserException($"Destination account id must be provided.");
            account = await accountsService.TryGetAccount(requestDto.DestinationAccount);
            if (account.HasValue)
                requestDto.DestinationAccountName = account.Value.Name;
            else
            {
                requestDto.DestinationAccountName ??= MidasConstants.FallbackAccountName;
                newAccounts.Add((requestDto.DestinationAccount, new AccountDto
                {
                    IsMine = false,
                    Name = requestDto.DestinationAccountName
                }));
            }

            var transaction = new ExtendedTransactionDto
            {
                SourceAccount = requestDto.SourceAccount,
                DestinationAccount = requestDto.DestinationAccount,
                Description = requestDto.Description ?? "",
                Amount = requestDto.Amount,
                Category = requestDto.Category ?? MidasConstants.DefaultCategory,
                Tags = requestDto.Tags.ToHashSet(),
                TimeStamp = requestDto.Date,
                SourceAccountName = requestDto.SourceAccountName,
                DestinationAccountName = requestDto.DestinationAccountName,
            };
            return (newAccounts, transaction);
        }

        [HttpPost("search/account-id")]
        public async Task<IResult> SearchAccountId(
            [FromForm(Name = "inputName")] string? inputName,
            [FromForm(Name = "Name")] string? accountName)
        {
            var result = new EditTransactionAccountId();
            if (!string.IsNullOrEmpty(inputName))
                result.InputName = inputName;

            if (string.IsNullOrEmpty(accountName))
                return await componentFactory.RenderComponentAsync(result);

            var accountId = await accountsService.GetAccountIdByName(accountName);
            if (accountId.HasValue)
            {
                result.Id = accountId.Value;
                result.IsExistingAccount = true;
            }
            else
                result.Id = Guid.NewGuid().ToString();
            return await componentFactory.RenderComponentAsync(result);
        }

        [HttpPost("edit/add-tag")]
        public Task<IResult> AddTag([FromForm] string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new UserException("Tag cannot be empty");
            return componentFactory.RenderComponentAsync(new EditTransactionTag { Text = tag.Trim() });
        }
    }
}
