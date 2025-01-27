using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Midas.Core.Exceptions;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Split.Services;
using Midas.Domain.Transactions.Services;
using Midas.UI.Components.Split;
using Midas.UI.Models.Split;
using Midas.UI.Shared.Controllers;
using Midas.UI.Shared.Exceptions;
using Midas.UI.Shared.ModelBinders;
using System.ComponentModel.DataAnnotations;

namespace Midas.UI.Controllers.Split
{
    [Route("split")]
    public class SplitController(
        ITransactionService transactionService,
        IAccountsService accountsService,
        ISplitService splitService,
        IComponentFactory componentFactory) : MidasUIController
    {

        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> Get([FromQuery(Name = "transactionId")] long? transactionId)
        {
            var split = new Midas.UI.Components.Split.Split();
            if (transactionId.HasValue)
            {
                var extendedTransaction = await transactionService.GetExtendedTransaction(accountsService, transactionId.Value);
                if (!extendedTransaction.HasValue)
                    throw new NotFoundPageException();
                split.ConfigureSplit = new ConfigureSplit()
                {
                    SourceTransaction = (transactionId.Value, extendedTransaction.Value),
                    SplitSourceData = false,
                };
            }
            return await componentFactory.RenderComponentAsync(split);
        }

        [HttpGet("init")]
        public async Task<IResult> InitializeSplit(
            [FromQuery, Required] long transactionId,
            [FromQuery(Name = "split-source-data-hashes"), ModelBinder(typeof(CheckboxModelBinder))] bool splitSourceHashes)
        {
            var extendedTransaction = await transactionService.GetExtendedTransaction(accountsService, transactionId);
            if (!extendedTransaction.HasValue)
                throw new UserException($"Transaction {transactionId} not found.");
            return await componentFactory.RenderComponentAsync(new ConfigureSplit
            {
                SourceTransaction = (transactionId, extendedTransaction.Value),
                SplitSourceData = splitSourceHashes
            });
        }

        [HttpGet("add-split")]
        public async Task<IResult> AddSplit([FromQuery, Required] long transactionId)
        {
            var extendedTransaction = await transactionService.GetExtendedTransaction(accountsService, transactionId);
            if (!extendedTransaction.HasValue)
                throw new UserException($"Transaction {transactionId} not found.");
            return await componentFactory.RenderComponentAsync(new AddedSplit
            {
                SourceTransaction = extendedTransaction.Value
            });
        }

        [HttpPost("dry-run")]
        public async Task<IResult> DryRunSplit([FromForm] SplitRequestDto request)
        {
            var splits = request.ParseSplits();
            var result = await splitService.DryRun(new()
            {
                Splits = request.ParseSplits(),
                SourceTransactionId = request.SourceTransactionId
            });

            return await componentFactory.RenderComponentAsync(new SplitDryRunResult
            {
                NewAccounts = result.NewAccounts.Values.ToList(),
                BalanceChanges = result.BalanceChanges,
                NewCategories = result.NewCategories,
                NewTags = result.NewTags,
                NewTransactions = result.NewTransactions,
                SplitsPayload = request.Splits,
                SourceTransactionId = request.SourceTransactionId
            });
        }

        [HttpPost("run")]
        public async Task<IResult> PerformSplit([FromForm] SplitRequestDto request)
        {
            var splits = request.ParseSplits();
            var result = await splitService.PerformSplit(new()
            {
                Splits = request.ParseSplits(),
                SourceTransactionId = request.SourceTransactionId
            });

            return await componentFactory.RenderComponentAsync(new SplitResult
            {
                NewTransactions = result,
            });
        }
    }
}
