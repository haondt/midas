using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Microsoft.Extensions.Options;
using SpendLess.Accounts.Services;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Models;
using SpendLess.Domain.Services;
using SpendLess.NodeRed.Models;
using SpendLess.NodeRed.Services;
using SpendLess.TransactionImport.Exceptions;
using SpendLess.TransactionImport.Models;
using SpendLess.TransactionImport.Storages;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Services;

namespace SpendLess.TransactionImport.Services
{
    public class TransactionImportService(IAsyncJobRegistry jobRegistry,
        INodeRedService nodeRed,
        IOptions<TransactionImportSettings> options,
        IAccountsService accountsService,
        ITransactionImportDataStorage importDataStorage,
        ITransactionService transactionService) : ITransactionImportService
    {


        public Result<T, (double, Optional<string>)> GetAsyncJobResult<T>(string jobId)
        {
            var (status, progress, message) = jobRegistry.GetJobProgress(jobId);
            if (status < AsyncJobStatus.Complete)
                return new((progress * 100, message));
            var result = jobRegistry.GetJobResult(jobId);
            if (!result.HasValue)
                throw new InvalidOperationException($"Job {jobId} has status {status} and no result.");
            if (result.Value is not T castedResult)
                throw new InvalidOperationException($"Job {jobId} has status {status} and a result of type {result.Value.GetType()} instead of {typeof(DryRunResultDto)}.");
            return new(castedResult);
        }

        public Result<DryRunResultDto, (double, Optional<string>)> GetDryRunResult(string jobId)
        {
            return GetAsyncJobResult<DryRunResultDto>(jobId);
        }

        public Result<TransactionImportResultDto, (double, Optional<string>)> GetImportResult(string jobId)
        {
            return GetAsyncJobResult<TransactionImportResultDto>(jobId);
        }

        public string StartImport(string dryRunId)
        {
            var result = jobRegistry.GetJobResult(dryRunId);
            if (!result.HasValue || result.Value is not DryRunResultDto dryRunResult)
                throw new InvalidOperationException($"Job {dryRunId} has a result of type {result.Value.GetType()} instead of {typeof(DryRunResultDto)}.");

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {
                var result = new TransactionImportResultDto
                {
                    TotalTransactions = dryRunResult.Transactions.Count,
                    ImportTag = dryRunResult.ImportTag
                };
                try
                {
                    foreach (var newAccount in dryRunResult.NewAccounts)
                        await accountsService.CreateAccount(newAccount.Key, new AccountDto
                        {
                            Name = newAccount.Value
                        }, false);

                    var batchSize = options.Value.StorageOperationBatchSize;
                    var batches = dryRunResult.Transactions.Chunk(batchSize);
                    foreach (var batch in batches)
                    {
                        var transactions = batch
                            .Where(t => t.TransactionData.HasValue)
                            .Select(t => new TransactionDto
                            {
                                Amount = t.TransactionData.Value.Amount,
                                Tags = t.TransactionData.Value.Tags,
                                Category = t.TransactionData.Value.Category,
                                SourceAccount = t.TransactionData.Value.Source.Id,
                                DestinationAccount = t.TransactionData.Value.Destination.Id,
                                Description = t.TransactionData.Value.Description,
                                TimeStamp = t.TransactionData.Value.TimeStamp,
                                SourceData = t.SourceData
                            })
                            .ToList();

                        var deleteTransactions = batch
                            .Where(q => q.ReplacementTarget.HasValue)
                            .Select(q => q.ReplacementTarget.Value)
                            .ToList();
                        var transactionIds = await transactionService.ReplaceTransactions(transactions, deleteTransactions);
                        await importDataStorage.SetMany(transactionIds
                            .Zip(batch)
                            .ToDictionary(t => t.First, t => t.Second.ImportData));

                        result.TotalTransactions += batch.Length;
                        jobRegistry.UpdateJobProgress(jobId, result.TotalTransactions / dryRunResult.Transactions.Count);
                    }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (Exception ex)
                {
                    result.Errors = [ex.ToString()];
                    result.IsSuccessful = false;
                    jobRegistry.FailJob(jobId, result);
                }
            });

            return jobId;
        }

        public string StartDryRun(
            List<TransactionFilter> filters,
            bool addImportTag,
            string? configurationSlug = null,
            string? accountId = null)
        {

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            var currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var importTag = $"import-{currentTimeStamp}";

            _ = Task.Run(async () =>
            {
                var result = new DryRunResultDto();
                if (addImportTag)
                    result.ImportTag = importTag;

                DryRunAccountDto? globalImportAccount = null;

                if (!string.IsNullOrEmpty(accountId))
                    globalImportAccount = new DryRunAccountDto
                    {
                        Id = accountId,
                        Name = (await accountsService.TryGetAccount(accountId))
                            .As(a => a.Name)
                            .Or(SpendLessConstants.FallbackAccountName)
                    };

                var results = new List<(SendToNodeRedRequestDto Request, SendToNodeRedResponseDto Response)>();
                var existingCategories = (await transactionService.GetCategories()).ToHashSet();
                var existingTags = (await transactionService.GetTags()).ToHashSet();
                try
                {
                    var pageSize = Math.Min(options.Value.NodeRedOperationBatchSize, options.Value.StorageOperationBatchSize);
                    var page = 1;
                    var transactions = await transactionService.GetPagedTransactions(filters, pageSize, page);
                    var totalTransactions = await transactionService.GetTransactionsCount(filters);
                    while (transactions.Count > 0)
                    {
                        Dictionary<long, TransactionImportData> importDatas = [];
                        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(configurationSlug))
                            importDatas = await importDataStorage.GetMany(transactions.Keys.ToList());

                        var payloads = new List<(SendToNodeRedRequestDto Request, long TransactionId)>();
                        foreach (var b in transactions)
                        {
                            payloads.Add((new SendToNodeRedRequestDto
                            {
                                Account = accountId ?? importDatas[b.Key].Account,
                                Configuration = configurationSlug ?? importDatas[b.Key].ConfigurationSlug,
                                Data = b.Value.SourceData
                            }, b.Key));
                        }

                        var batchResults = await Task.WhenAll(payloads.Select(async p =>
                        {
                            try
                            {
                                return (p, await nodeRed.SendToNodeRed(p.Request, cancellationToken));
                            }
                            catch (Exception ex)
                            {
                                throw new SendToNodeRedException($"{ex.GetType()}: {ex.Message}", ex)
                                {
                                    Request = p.Request,
                                };
                            }
                        }));

                        foreach (var (source, response) in batchResults)
                        {
                            var context = new TransactionImportDryRunContext
                            {
                                Result = result,
                                CurrentTimeStamp = currentTimeStamp,
                                ExistingCategories = existingCategories,
                                ExistingTags = existingTags,
                            };

                            await ProcessNodeRedResult(source.Request, response, context);
                            context.Result.Transactions[^1].ReplacementTarget = source.TransactionId;
                        }

                        foreach (var batch in result.Transactions.Chunk(options.Value.StorageOperationBatchSize))
                        {
                            var hasBeenImported = await transactionService.CheckIfTransactionsHaveBeenImported(batch.Select(r => r.SourceData).ToList());
                            foreach (var resultDto in batch
                                .Zip(hasBeenImported))
                            {
                                if (resultDto.First.ReplacementTarget.HasValue)
                                    resultDto.First.Warnings.Add(TransactionImportWarning.WillUpdateExisting);
                                else if (resultDto.Second)
                                    resultDto.First.Warnings.Add(TransactionImportWarning.SourceDataHashExists);
                            }
                        }

                        jobRegistry.UpdateJobProgress(jobId, (double)results.Count / (double)totalTransactions);


                        page++;
                        transactions = await transactionService.GetPagedTransactions(filters, pageSize, page);
                    }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (SendToNodeRedException ex)
                {
                    result.Transactions = new List<DryRunTransactionDto>
                    {
                        new DryRunTransactionDto
                        {
                            SourceRequestPayload = ex.Request.ToString(),
                            SourceData = ex.Request.Data,
                            Errors = new HashSet<string>
                            {
                                ex.Message
                            },
                            ImportData = new TransactionImportData
                            {
                                Account = ex.Request.Account,
                                ConfigurationSlug = ex.Request.Configuration
                            }
                        }
                    };
                    jobRegistry.FailJob(jobId, result, true);
                    throw;
                }
                catch (Exception)
                {
                    jobRegistry.FailJob(jobId, requestCancellation: true);
                    throw;
                }
            });

            return jobId;
        }

        public string StartDryRun(
            List<List<string>> csvData,
            bool addImportTag,
            string configurationSlug,
            string accountId,
            TransactionImportConflictResolutionStrategy conflictResolutionStrategy)
        {

            var (jobId, cancellationToken) = jobRegistry.RegisterJob("parsing csv...");
            var currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var importTag = $"import-{currentTimeStamp}";

            _ = Task.Run(async () =>
            {

                var importAccount = new DryRunAccountDto
                {
                    Id = accountId,
                    Name = (await accountsService.TryGetAccount(accountId))
                        .As(a => a.Name)
                        .Or(SpendLessConstants.FallbackAccountName)
                };
                var importData = new TransactionImportData
                {
                    Account = importAccount.Id,
                    ConfigurationSlug = configurationSlug
                };

                var result = new DryRunResultDto();

                if (addImportTag)
                    result.ImportTag = importTag;

                var results = new List<(SendToNodeRedRequestDto Request, SendToNodeRedResponseDto Response)>();

                if (csvData.Count == 0)
                {
                    jobRegistry.CompleteJob(jobId, result);
                    return;
                }

                var existingCategories = (await transactionService.GetCategories()).ToHashSet();
                var existingTags = (await transactionService.GetTags()).ToHashSet();

                try
                {
                    var batchSize = options.Value.NodeRedOperationBatchSize;
                    //var header = csvData.First();
                    var batches = csvData.Chunk(batchSize);

                    var hasBeenImported = new List<bool>();
                    var processed = 0;
                    foreach (var batch in batches)
                    {
                        List<List<string>> filteredBatch = batch.ToList();
                        var batchedHasBeenImported = await transactionService.CheckIfTransactionsHaveBeenImported(filteredBatch);
                        if (conflictResolutionStrategy == TransactionImportConflictResolutionStrategy.Omit)
                            filteredBatch = batch.Where((b, i) => !batchedHasBeenImported[i]).ToList();
                        else
                            hasBeenImported.AddRange(batchedHasBeenImported);

                        var payloads = filteredBatch.Select(b => new SendToNodeRedRequestDto
                        {
                            Account = accountId,
                            Configuration = configurationSlug,
                            Data = b
                        });

                        var batchResults = await Task.WhenAll(payloads.Select(async p =>
                        {
                            try
                            {
                                return (p, await nodeRed.SendToNodeRed(p, cancellationToken));
                            }
                            catch (Exception ex)
                            {
                                throw new SendToNodeRedException($"{ex.GetType()}: {ex.Message}", ex)
                                {
                                    Request = p
                                };
                            }
                        }));
                        results.AddRange(batchResults);
                        processed += batch.Length;
                        jobRegistry.UpdateJobProgress(jobId, (double)processed / (double)csvData.Count);
                    }

                    jobRegistry.UpdateJobProgress(jobId, (double)processed / (double)csvData.Count);

                    var context = new TransactionImportDryRunContext
                    {
                        CurrentTimeStamp = currentTimeStamp,
                        ExistingCategories = existingCategories,
                        ExistingTags = existingTags,
                        Result = result
                    };
                    foreach (var (request, response) in results)
                        await ProcessNodeRedResult(request, response, context);

                    if (conflictResolutionStrategy == TransactionImportConflictResolutionStrategy.Warn)
                        foreach (var batch in result.Transactions.Chunk(options.Value.StorageOperationBatchSize))
                        {
                            foreach (var resultDto in batch
                                .Zip(hasBeenImported)
                                .Where(zipped => zipped.Second)
                                .Select(zipped => zipped.First))
                                resultDto.Warnings.Add(TransactionImportWarning.SourceDataHashExists);
                        }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (SendToNodeRedException ex)
                {
                    result.Transactions = new List<DryRunTransactionDto>
                    {
                        new DryRunTransactionDto
                        {
                            SourceRequestPayload = ex.Request.ToString(),
                            SourceData = ex.Request.Data,
                            Errors = new HashSet<string>
                            {
                                ex.Message
                            },
                            ImportData = new TransactionImportData
                            {
                                Account = ex.Request.Account,
                                ConfigurationSlug = ex.Request.Configuration
                            }
                        }
                    };
                    jobRegistry.FailJob(jobId, result, true);
                    throw;
                }
                catch (Exception)
                {
                    jobRegistry.FailJob(jobId, requestCancellation: true);
                    throw;
                }
            });

            return jobId;
        }

        public async Task ProcessNodeRedResult(SendToNodeRedRequestDto request, SendToNodeRedResponseDto response, TransactionImportDryRunContext context)
        {
            var resultDto = new DryRunTransactionDto
            {
                SourceRequestPayload = request.ToString(),
                SourceData = request.Data,
                ImportData = new()
                {
                    Account = request.Account,
                    ConfigurationSlug = request.Configuration
                },
            };

            // given an existing id, fetch the name
            if (response.Transaction.Source.Id != null &&
                (await accountsService.TryGetAccount(response.Transaction.Source.Id)).Test(out var account))
            {
                response.Transaction.Source.Name = account.Name;
            }
            // given a new id, either grab the name from the new accounts
            // or default it to the fallback name and add it to the new accounts
            else if (response.Transaction.Source.Id != null)
            {
                if (context.Result.NewAccounts.TryGetValue(response.Transaction.Source.Id, out var name))
                    response.Transaction.Source.Name = name;
                else
                    context.Result.NewAccounts[response.Transaction.Source.Id] = response.Transaction.Source.Name ??= SpendLessConstants.FallbackAccountName;
            }
            // given a name sans id, try and find the id based on the name in the new accounts
            // otherwise generate a new id and add it to the new accounts
            // also warn if we are going to create an account with the same name as an existing one
            else if (response.Transaction.Source.Name != null)
            {
                foreach (var (key, value) in context.Result.NewAccounts)
                    if (value == response.Transaction.Source.Name)
                    {
                        response.Transaction.Source.Id = key;
                        break;
                    }
                if (response.Transaction.Source.Id == null)
                {
                    response.Transaction.Source.Id = Guid.NewGuid().ToString();
                    context.Result.NewAccounts[response.Transaction.Source.Id] = response.Transaction.Source.Name;
                    if (await accountsService.HasAccountWithName(response.Transaction.Source.Name))
                        resultDto.Warnings.Add($"{TransactionImportWarning.CreatingAccountWithSameNameAsExisting} Name: {response.Transaction.Source.Name}.");
                }
            }
            // no id, no name, using defaults
            else
            {
                resultDto.Warnings.Add(TransactionImportWarning.MissingSourceAccount);
                response.Transaction.Source.Id = SpendLessConstants.DefaultAccount;
                response.Transaction.Source.Name = SpendLessConstants.DefaultAccountName;
            }

            // given an existing id, fetch the name
            if (response.Transaction.Destination.Id != null &&
                (await accountsService.TryGetAccount(response.Transaction.Destination.Id)).Test(out account))
            {
                response.Transaction.Destination.Name = account.Name;
            }
            // given a new id, either grab the name from the new accounts
            // or default it to the fallback name and add it to the new accounts
            else if (response.Transaction.Destination.Id != null)
            {
                if (context.Result.NewAccounts.TryGetValue(response.Transaction.Destination.Id, out var name))
                    response.Transaction.Destination.Name = name;
                else
                    context.Result.NewAccounts[response.Transaction.Destination.Id] = response.Transaction.Destination.Name ??= SpendLessConstants.FallbackAccountName;
            }
            // given a name sans id, try and find the id based on the name in the new accounts
            // otherwise generate a new id and add it to the new accounts
            // also warn if we are going to create an account with the same name as an existing one
            else if (response.Transaction.Destination.Name != null)
            {
                foreach (var (key, value) in context.Result.NewAccounts)
                    if (value == response.Transaction.Destination.Name)
                    {
                        response.Transaction.Destination.Id = key;
                        break;
                    }
                if (response.Transaction.Destination.Id == null)
                {
                    response.Transaction.Destination.Id = Guid.NewGuid().ToString();
                    context.Result.NewAccounts[response.Transaction.Destination.Id] = response.Transaction.Destination.Name;
                    if (await accountsService.HasAccountWithName(response.Transaction.Destination.Name))
                        resultDto.Warnings.Add($"{TransactionImportWarning.CreatingAccountWithSameNameAsExisting} Name: {response.Transaction.Destination.Name}.");
                }
            }
            // no id, no name, using defaults
            else
            {
                resultDto.Warnings.Add(TransactionImportWarning.MissingDestinationAccount);
                response.Transaction.Destination.Id = SpendLessConstants.DefaultAccount;
                response.Transaction.Destination.Name = SpendLessConstants.DefaultAccountName;
            }


            resultDto.TransactionData = new DryRunTransactionDataDto
            {
                Amount = response.Transaction.Amount,
                TimeStamp = response.Transaction.TimeStamp.HasValue ? response.Transaction.TimeStamp.Value : context.CurrentTimeStamp,
                Category = response.Transaction.Category,
                Tags = response.Transaction.Tags,
                Source = new DryRunAccountDto
                {
                    Id = response.Transaction.Source.Id,
                    Name = response.Transaction.Source.Name
                },
                Destination = new DryRunAccountDto
                {
                    Id = response.Transaction.Destination.Id,
                    Name = response.Transaction.Destination.Name
                },
                Description = response.Transaction.Description
            };

            if (context.Result.ImportTag.HasValue)
                resultDto.TransactionData.Value.Tags.Add(context.Result.ImportTag.Value);

            context.Result.Transactions.Add(resultDto);

            if (resultDto.TransactionData.Value.Source.Id == request.Account)
                context.Result.BalanceChange -= resultDto.TransactionData.Value.Amount;
            else if (resultDto.TransactionData.Value.Destination.Id == request.Account)
                context.Result.BalanceChange += resultDto.TransactionData.Value.Amount;

            foreach (var tag in resultDto.TransactionData.Value.Tags)
            {
                if (context.ExistingTags.Contains(tag))
                    continue;
                if (context.Result.NewTags.ContainsKey(tag))
                    context.Result.NewTags[tag] += 1;
                else
                    context.Result.NewTags[tag] = 1;
            }

            if (resultDto.TransactionData.Value.Category == SpendLessConstants.DefaultCategory)
                resultDto.Warnings.Add(TransactionImportWarning.MissingCategory);
            else if (!context.ExistingCategories.Contains(resultDto.TransactionData.Value.Category))
            {
                if (context.Result.NewCategories.ContainsKey(resultDto.TransactionData.Value.Category))
                    context.Result.NewCategories[resultDto.TransactionData.Value.Category] += 1;
                else
                    context.Result.NewCategories[resultDto.TransactionData.Value.Category] = 1;
            }
        }
    }
}
