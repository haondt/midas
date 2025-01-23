using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Constants;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Reconcile.Models;
using Midas.Domain.Reconcile.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;
using Midas.UI.Components.Reconcile;
using Midas.UI.Models.Reconcile;
using Midas.UI.Services.Transactions;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Reconcile
{
    [Route("reconcile")]
    public class TransactionReconcileController(
        IReconcileService reconcileService,
        IAccountsService accountsService,
        ITransactionImportDataStorage importDataStorage,
        ITransactionFilterService transactionFilterService,
        IComponentFactory componentFactory) : MidasUIController
    {

        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Midas.UI.Components.Reconcile.Reconcile>();
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

        [HttpGet("dry-run/{id}")]
        public async Task<IResult> GetDryRunStatus(string id)
        {

            var result = reconcileService.GetDryRunResult(id);
            if (!result.IsSuccessful)
            {
                // todo: indeterminate?
                return await componentFactory.RenderComponentAsync(new ProgressPanel
                {
                    CallbackEndpoint = $"/reconcile/dry-run/{id}",
                    ProgressPercent = result.Reason.Progress
                });
            }

            if (!result.Value.MergedTransactions.IsSuccessful)
                return await componentFactory.RenderComponentAsync(new ReconcileDryRunResult
                {
                    JobId = id,
                    Result = new() { MergedTransactions = new(result.Value.MergedTransactions.Reason) }
                });


            var accountIds = result.Value.MergedTransactions.Value.SelectMany(t => new List<string> { t.NewTransaction.SourceAccount, t.NewTransaction.DestinationAccount });
            var accountNames = (await accountsService.GetMany(accountIds.ToList()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Name);
            var allOldTransactions = result.Value.MergedTransactions.Value.SelectMany(t => t.OldTransactions).ToList();
            var sourceDataHashes = (await importDataStorage.GetMany(allOldTransactions)).Zip(allOldTransactions).ToDictionary(q => q.Second, q => q.First.Select(r => r.SourceDataHash));

            //var mergedTransactions = new List<ReconcileDryRunExpandedSingleResult>();
            //foreach(var t in result.Value.MergedTransactions.Value)
            //{
            //    mergedTransactions.Add(new ()
            //    {
            //        NewTransaction = new()
            //        {
            //            Amount = t.NewTransaction.Amount,
            //            DestinationAccount = t.NewTransaction.DestinationAccount,
            //            SourceAccount = t.NewTransaction.SourceAccount,
            //            Category = t.NewTransaction.Category,
            //            Description = t.NewTransaction.Description,
            //            Tags = t.NewTransaction.Tags,
            //            TimeStamp = t.NewTransaction.TimeStamp,
            //            SourceAccountName = accountNames.GetValueOrDefault(t.NewTransaction.SourceAccount, MidasConstants.DefaultAccountName),
            //            DestinationAccountName = accountNames.GetValueOrDefault(t.NewTransaction.DestinationAccount, MidasConstants.DefaultAccountName),

            //        }
            //    })
            //}
            //var expandedResult = new ReconcileDryRunExpandedResultDto { MergedTransactions = }

            var expandedResult = new ReconcileDryRunExpandedResultDto
            {
                MergedTransactions = new(result.Value.MergedTransactions.Value.Select(t => new ReconcileDryRunExpandedSingleResult
                {
                    OldTransactions = t.OldTransactions,
                    NewTransaction = new()
                    {
                        Amount = t.NewTransaction.Amount,
                        DestinationAccount = t.NewTransaction.DestinationAccount,
                        SourceAccount = t.NewTransaction.SourceAccount,
                        Category = t.NewTransaction.Category,
                        Description = t.NewTransaction.Description,
                        Tags = t.NewTransaction.Tags,
                        TimeStamp = t.NewTransaction.TimeStamp,
                        SourceAccountName = accountNames.GetValueOrDefault(t.NewTransaction.SourceAccount, MidasConstants.DefaultAccountName),
                        DestinationAccountName = accountNames.GetValueOrDefault(t.NewTransaction.DestinationAccount, MidasConstants.DefaultAccountName),
                        SourceDataHashes = t.OldTransactions.SelectMany(t => sourceDataHashes.GetValueOrDefault(t, [])).Distinct().ToList()
                    }
                }).ToList())
            };

            return await componentFactory.RenderComponentAsync(new ReconcileDryRunResult
            {
                JobId = id,
                Result = expandedResult
            });
        }

        [HttpPost("run/{id}")]
        public async Task<IResult> StartMerge(string id)
        {
            var jobId = reconcileService.StartMerge(id);

            return await componentFactory.RenderComponentAsync(new ProgressPanel
            {
                CallbackEndpoint = $"/reconcile/run/{jobId}",
                ProgressPercent = 0
            });
        }

        [HttpGet("run/{id}")]
        public async Task<IResult> GetMergeStatus(string id)
        {

            var result = reconcileService.GetMergeResult(id);
            if (!result.IsSuccessful)
            {
                // todo: indeterminate?
                return await componentFactory.RenderComponentAsync(new ProgressPanel
                {
                    CallbackEndpoint = $"/reconcile/run/{id}",
                    ProgressPercent = result.Reason.Progress
                });
            }

            return await componentFactory.RenderComponentAsync(new ReconcileResult
            {
                Result = result.Value
            });
        }
    }
}
