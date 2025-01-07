using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Domain.Reconcile.Models;
using SpendLess.Domain.Reconcile.Services;
using SpendLess.Persistence.Models;
using SpendLess.UI.Components.Reconcile;
using SpendLess.UI.Models.Reconcile;
using SpendLess.UI.Services.Transactions;
using SpendLess.UI.Shared.Components;
using SpendLess.UI.Shared.Controllers;

namespace SpendLess.UI.Controllers.Reconcile
{
    [Route("reconcile")]
    public class TransactionReconcileController(
        IReconcileService reconcileService,
        ITransactionFilterService transactionFilterService,
        IComponentFactory componentFactory) : SpendLessUIController
    {

        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<TransactionReconcile>();
        }

        [HttpPost("dry-run")]
        public async Task<IResult> DryRun([FromForm] ReconcileDryRunRequestDto request)
        {

            var parsedFilters = (await transactionFilterService.ParseFiltersAsync(request.Filters)).ToList();
            if (!string.IsNullOrEmpty(request.Transactions))
                parsedFilters.Add(TransactionFilter.TransactionIdIsOneOf(request.Transactions
                    .Split(',')
                    .Select(q => long.Parse(q))
                    .ToList()));

            var options = new ReconcileDryRunOptions
            {
                JoinCategoryStrategy = request.JoinCategoryStrategy,
                JoinDateStrategy = request.JoinDateStrategy,
                JoinDescriptionStrategy = request.JoinDescriptionStrategy,
                PairingDateToleranceInDays = request.PairingDateToleranceInDays,
                PairingMatchDate = request.PairingMatchDate,
                PairingMatchDescription = request.PairingMatchDescription,
                Filters = parsedFilters
            };

            var jobId = reconcileService.StartDryRun(options);

            return await componentFactory.RenderComponentAsync(new ProgressPanel
            {
                CallbackEndpoint = $"/reconcile/dry-run/{jobId}",
                ProgressPercent = 0
            });
        }
    }
}
