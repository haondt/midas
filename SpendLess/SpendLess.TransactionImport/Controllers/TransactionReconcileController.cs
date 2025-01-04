using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpendLess.TransactionImport.Components;
using SpendLess.TransactionImport.Models;
using SpendLess.TransactionImport.Services;
using SpendLess.Transactions.Controllers;
using SpendLess.Transactions.Models;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.TransactionImport.Controllers
{
    [Route("reconcile")]
    public class TransactionReconcileController(
        IReconcileService reconcileService,
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

            var parsedFilters = TransactionsController.ParseFilters(request.Filters).ToList();
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
