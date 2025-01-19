using Haondt.Core.Models;
using Midas.Core.Constants;
using Midas.Core.Models;
using Midas.Domain.Reconcile.Models;
using Midas.Domain.Shared.Services;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;
using Midas.Persistence.Storages.Abstractions;

namespace Midas.Domain.Reconcile.Services
{
    public class ReconcileService(
        IAsyncJobRegistry jobRegistry,
        ITransactionService transactionService,
        ITransactionImportDataStorage importDataStorage
        ) : IReconcileService
    {
        public Result<ReconcileDryRunResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId)
        {
            return jobRegistry.GetJobResultOrProgress<ReconcileDryRunResultDto>(jobId);
        }

        public Result<Result<ReconcileMergeResultDto, string>, (double Progress, Optional<string> ProgressMessage)> GetMergeResult(string jobId)
        {
            return jobRegistry.GetJobResultOrProgress<Result<ReconcileMergeResultDto, string>>(jobId);
        }

        public string StartDryRun(ReconcileDryRunOptions options)
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();

            _ = Task.Run(async () =>
            {
                try
                {
                    var eligibleTransactions = (await transactionService.GetTransactions(options.Filters)).ToList();
                    var transactionsBySource = new Dictionary<string, List<long>>();
                    var transactionsByDestination = new Dictionary<string, List<long>>();
                    foreach (var (k, v) in eligibleTransactions)
                    {
                        if (!transactionsBySource.TryGetValue(v.SourceAccount, out var sourceList))
                            sourceList = transactionsBySource[v.SourceAccount] = [];
                        sourceList.Add(k);
                        if (!transactionsByDestination.TryGetValue(v.DestinationAccount, out var destinationList))
                            destinationList = transactionsByDestination[v.DestinationAccount] = [];
                        destinationList.Add(k);
                    }

                    var transactionCombinations = new List<(KeyValuePair<long, TransactionDto> First, KeyValuePair<long, TransactionDto> Second)>();

                    for (int i = 0; i < eligibleTransactions.Count; i++)
                        for (int j = 0; j < eligibleTransactions.Count; j++)
                            transactionCombinations.Add((eligibleTransactions[i], eligibleTransactions[j]));


                    var pairs = transactionCombinations
                        .Where(pair => pair.First.Key != pair.Second.Key)
                        .Where(pair => pair.First.Value.Amount == pair.Second.Value.Amount)
                        .Where(pair => pair.First.Value.SourceAccount != pair.First.Value.DestinationAccount)
                        .Where(pair => pair.Second.Value.SourceAccount != pair.Second.Value.DestinationAccount)
                        .Where(pair => pair.First.Value.SourceAccount != pair.Second.Value.DestinationAccount)
                        .Where(pair => pair.First.Value.DestinationAccount == pair.Second.Value.SourceAccount);

                    if (options.PairingMatchDescription)
                        pairs = pairs.Where(pair => pair.First.Value.Description == pair.Second.Value.Description);

                    if (options.PairingMatchDate)
                        pairs = pairs.Where(pair => Math.Abs((pair.First.Value.TimeStamp - pair.Second.Value.TimeStamp).TotalDays) <= options.PairingDateToleranceInDays);

                    var appearances = new Dictionary<long, int>();
                    foreach (var pair in pairs)
                    {
                        appearances[pair.First.Key] = appearances.GetValueOrDefault(pair.First.Key, 0) + 1;
                        appearances[pair.Second.Key] = appearances.GetValueOrDefault(pair.Second.Key, 0) + 1;
                    }

                    pairs = pairs.Where(pair => appearances[pair.First.Key] == 1 && appearances[pair.Second.Key] == 1);

                    var mergedTransactions = pairs.Select(pair =>
                        new ReconcileDryRunSingleResult
                        {
                            NewTransaction = new TransactionDto
                            {
                                Amount = pair.First.Value.Amount,
                                SourceAccount = pair.First.Value.SourceAccount,
                                DestinationAccount = pair.Second.Value.DestinationAccount,
                                Category = options.JoinCategoryStrategy switch
                                {
                                    CategoryJoiningStrategy.Source => pair.First.Value.Category,
                                    CategoryJoiningStrategy.Destination => pair.Second.Value.Category,
                                    CategoryJoiningStrategy.Clear => MidasConstants.DefaultCategory,
                                    _ => throw new ArgumentOutOfRangeException($"Unkown {nameof(CategoryJoiningStrategy)}: {options.JoinCategoryStrategy}")
                                },
                                Description = options.JoinDescriptionStrategy switch
                                {
                                    DescriptionJoiningStrategy.Source => pair.First.Value.Description,
                                    DescriptionJoiningStrategy.Destination => pair.Second.Value.Description,
                                    DescriptionJoiningStrategy.Concatenate => $"{pair.First.Value.Description} | {pair.Second.Value.Description}",
                                    _ => throw new ArgumentOutOfRangeException($"Unkown {nameof(DescriptionJoiningStrategy)}: {options.JoinDescriptionStrategy}")
                                },
                                Tags = pair.First.Value.Tags.Concat(pair.Second.Value.Tags).ToHashSet(),
                                TimeStamp = options.JoinDateStrategy switch
                                {
                                    DateJoiningStrategy.Source => pair.First.Value.TimeStamp,
                                    DateJoiningStrategy.Destination => pair.Second.Value.TimeStamp,
                                    DateJoiningStrategy.Average => AbsoluteDateTime.Create(pair.First.Value.TimeStamp.UnixTimeSeconds + (pair.Second.Value.TimeStamp.UnixTimeSeconds - pair.First.Value.TimeStamp.UnixTimeSeconds) / 2),
                                    _ => throw new ArgumentOutOfRangeException($"Unkown {nameof(DateJoiningStrategy)}: {options.JoinDateStrategy}")

                                }
                            },
                            OldTransactions = [pair.First.Key, pair.Second.Key]
                        });

                    var result = new ReconcileDryRunResultDto
                    {
                        MergedTransactions = new(mergedTransactions.ToList())
                    };

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (Exception ex)
                {
                    jobRegistry.FailJob(jobId, new ReconcileDryRunResultDto { MergedTransactions = new(ex.ToString()) }, requestCancellation: true);
                    throw;
                }
            });

            return jobId;
        }

        public string StartMerge(string id)
        {
            var result = jobRegistry.GetJobResult(id);
            if (!result.HasValue || result.Value is not ReconcileDryRunResultDto dryRunResult)
                throw new InvalidOperationException($"Job {id} has a result of type {result.Value.GetType()} instead of {typeof(ReconcileDryRunResultDto)}.");
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();

            _ = Task.Run(async () =>
            {
                var mergeResult = new ReconcileMergeResultDto();
                try
                {
                    if (!dryRunResult.MergedTransactions.IsSuccessful)
                        throw new InvalidOperationException($"Cannot perform merge as dry run was not completed successfully.");
                    var newTransactions = dryRunResult.MergedTransactions.Value.Select(t => t.NewTransaction);
                    var oldTransactions = dryRunResult.MergedTransactions.Value.Select(t => t.OldTransactions);
                    var flattenedOldTransactions = oldTransactions.SelectMany(t => t).ToList();
                    var newTransactionMap = dryRunResult.MergedTransactions.Value.SelectMany(t => t.OldTransactions.Select(o => (o, t)))
                    .ToDictionary(q => q.o, q => q.t);
                    var oldImportData = (await importDataStorage.GetMany(flattenedOldTransactions))
                        .Zip(flattenedOldTransactions)
                        .ToDictionary(q => q.Second, q => q.First);
                    var newTransactionIds = await transactionService.ReplaceTransactions(newTransactions.ToList(), flattenedOldTransactions);
                    await importDataStorage.AddMany(newTransactionIds
                        .Zip(oldTransactions)
                        .SelectMany(q => q.Second
                            .SelectMany(r => oldImportData.GetValueOrDefault(r, [])
                                .Select(s => new TransactionImportDataDto
                                {
                                    Account = s.Account,
                                    ConfigurationSlug = s.ConfigurationSlug,
                                    SourceData = s.SourceData,
                                    Transaction = q.First
                                }))));
                    mergeResult.TotalMerges = newTransactionIds.Count;
                    jobRegistry.CompleteJob(jobId, new Result<ReconcileMergeResultDto, string>(mergeResult));
                }
                catch (Exception ex)
                {
                    jobRegistry.FailJob(jobId, new Result<ReconcileMergeResultDto, string>(ex.ToString()), requestCancellation: true);
                }
            });
            return jobId;
        }
    }
}
