using Haondt.Core.Models;
using SpendLess.Core.Constants;
using SpendLess.Domain.Reconcile.Models;
using SpendLess.Domain.Shared.Services;
using SpendLess.Domain.Transactions.Services;
using SpendLess.Persistence.Models;

namespace SpendLess.Domain.Reconcile.Services
{
    public class ReconcileService(
        IAsyncJobRegistry jobRegistry,
        ITransactionService transactionService
        ) : IReconcileService
    {
        public Result<ReconcileDryRunResultDto, (double Progress, Optional<string> ProgressMessage)> GetDryRunResult(string jobId)
        {
            return jobRegistry.GetJobResultOrProgress<ReconcileDryRunResultDto>(jobId);
        }

        public string StartDryRun(ReconcileDryRunOptions options)
        {
            var (jobId, cancellationToken) = jobRegistry.RegisterJob();

            _ = Task.Run(async () =>
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
                    pairs = pairs.Where(pair => TimeSpan.FromSeconds(pair.First.Value.TimeStamp - pair.Second.Value.TimeStamp).TotalDays <= options.PairingDateToleranceInDays);

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
                                CategoryJoiningStrategy.Clear => SpendLessConstants.DefaultCategory,
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
                                DateJoiningStrategy.Average => (pair.First.Value.TimeStamp + pair.Second.Value.TimeStamp) / 2,
                                _ => throw new ArgumentOutOfRangeException($"Unkown {nameof(DateJoiningStrategy)}: {options.JoinDateStrategy}")

                            }
                        },
                        OldTransactions = [pair.First.Key, pair.Second.Key]
                    });

                var result = new ReconcileDryRunResultDto
                {
                    MergedTransactions = mergedTransactions.ToList()
                };

                jobRegistry.CompleteJob(jobId, result);
            });

            return jobId;
        }
    }
}
