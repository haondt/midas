using Haondt.Core.Extensions;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Constants;
using Midas.Domain.Merge.Models;
using Midas.Domain.Merge.Services;
using Midas.Persistence.Models;
using Midas.UI.Components.Merge;
using Midas.UI.Models.Merge;
using Midas.UI.Services.Transactions;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Merge
{
    [Route("merge")]
    public class TransactionMergeController(
        IMergeService mergeService,
        ITransactionFilterService transactionFilterService,
        IComponentFactory componentFactory) : MidasUIController
    {

        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Midas.UI.Components.Merge.Merge>();
        }

        [HttpGet("configure")]
        public async Task<IResult> GetConfigurationSection(
            [FromQuery] IEnumerable<string> filters,
            [FromQuery] string? transactions)
        {
            var parsedFilters = (await transactionFilterService.ParseFiltersAsync(filters)).ToList();
            if (!string.IsNullOrEmpty(transactions))
                parsedFilters.Add(TransactionFilter.TransactionIdIsOneOf(transactions
                    .Split(',')
                    .Select(q => long.Parse(q))
                    .ToList()));

            var defaults = await mergeService.CreateDefaults(parsedFilters);
            return await componentFactory.RenderComponentAsync(new ConfigureMerge
            {
                Amount = defaults.Amount,
                MeanAmount = defaults.MeanAmount,
                SumAmount = defaults.SumAmount,
                SourceAccount = defaults.SourceAccount.Id,
                SourceAccountName = defaults.SourceAccount.Name,
                DestinationAccount = defaults.DestinationAccount.Id,
                DestinationAccountName = defaults.DestinationAccount.Name,
                Category = defaults.Category,
                ConcatenatedDescription = defaults.ConcatenatedDescription,
                CurrentTimestamp = defaults.CurrentTimestamp,
                Description = defaults.Description,
                FirstTimestamp = defaults.FirstTimestamp,
                LastTimestamp = defaults.LastTimestamp,
                MeanTimestamp = defaults.MeanTimestamp,
                Tags = defaults.Tags,
                Timestamp = defaults.Timestamp
            });
        }

        public async Task<TransactionMergeDryRunOptions> ParseDryRunRequestDto(TransactionMergeDryRunRequestDto request)
        {

            var parsedFilters = (await transactionFilterService.ParseFiltersAsync(request.Filters)).ToList();
            if (!string.IsNullOrEmpty(request.Transactions))
                parsedFilters.Add(TransactionFilter.TransactionIdIsOneOf(request.Transactions
                    .Split(',')
                    .Select(q => long.Parse(q))
                    .ToList()));

            var destinationAccountId = request.DestinationAccountId;
            if (string.IsNullOrEmpty(destinationAccountId) || destinationAccountId == MidasConstants.DefaultAccount)
                destinationAccountId = Guid.NewGuid().ToString();
            var sourceAccountId = request.SourceAccountId;
            if (string.IsNullOrEmpty(sourceAccountId) || sourceAccountId == MidasConstants.DefaultAccount)
                sourceAccountId = Guid.NewGuid().ToString();

            return new TransactionMergeDryRunOptions
            {
                Amount = request.Amount,
                DestinationAccountId = destinationAccountId,
                PreferredDestinationAccountName = !string.IsNullOrWhiteSpace(request.DestinationAccountName)
                    ? request.DestinationAccountName.Trim() : MidasConstants.FallbackAccountName,
                SourceAccountId = sourceAccountId,
                PreferredSourceAccountName = !string.IsNullOrWhiteSpace(request.SourceAccountName)
                    ? request.SourceAccountName.Trim() : MidasConstants.FallbackAccountName,
                Category = request.Category ?? MidasConstants.DefaultCategory,
                Description = request.Description ?? "",
                TargetTransactionFilters = parsedFilters,
                Tags = request.Tags,
                Timestamp = request.Timestamp,
            };
        }

        [HttpPost("dry-run")]
        public async Task<IResult> DryRun([FromForm] TransactionMergeDryRunRequestDto request)
        {
            var options = await ParseDryRunRequestDto(request);
            var result = await mergeService.DryRun(options);

            return await componentFactory.RenderComponentAsync(new MergeDryRunResult
            {
                NewAccounts = result.NewAccounts.Values.ToList(),
                NewCategories = result.NewCategory.As(q => new List<string> { q }).Or([]),
                NewTags = result.NewTags,
                BalanceChanges = result.BalanceChanges,
                Amount = result.Amount,
                Tags = result.Tags,
                Category = result.Category,
                Timestamp = result.Timestamp,
                SourceAccountName = result.SourceAccount.Name,
                DestinationAccountName = result.DestinationAccount.Name,
                Description = result.Description
            });
        }

        [HttpPost("run")]
        public async Task<IResult> PerformMerge([FromForm] TransactionMergeDryRunRequestDto request)
        {
            var options = await ParseDryRunRequestDto(request);
            var id = await mergeService.PerformMerge(options);

            Response.AsResponseData()
                .HxLocation($"/transactions/edit/{id}", target: "#content");
            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "Transactions merged successfully!",
                Severity = Shared.Models.ToastSeverity.Success
            });
        }
    }
}
