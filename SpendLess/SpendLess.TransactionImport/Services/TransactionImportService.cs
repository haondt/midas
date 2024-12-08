using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using SpendLess.Core.Extensions;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Models;
using SpendLess.Domain.Services;
using SpendLess.NodeRed.Models;
using SpendLess.NodeRed.Services;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.TransactionImport.Exceptions;
using SpendLess.TransactionImport.Models;

namespace SpendLess.TransactionImport.Services
{
    public class TransactionImportService(IAsyncJobRegistry jobRegistry,
        INodeRedService nodeRed,
        ISingleTypeSpendLessStorage<AccountDto> accountStorage,
        ISingleTypeSpendLessStorage<CategoryDto> categoryStorage,
        ISingleTypeSpendLessStorage<TagDto> tagStorage) : ITransactionImportService
    {

        public Result<SendToNodeRedResultDto, (double, Optional<string>)> GetDryRunResult(string jobId)
        {
            var (status, progress, message) = jobRegistry.GetJobProgress(jobId);
            if (status < AsyncJobStatus.Complete)
                return new((progress * 100, message));
            var result = jobRegistry.GetJobResult(jobId);
            if (!result.HasValue)
                throw new InvalidOperationException($"Job {jobId} has status {status} and no result.");
            if (result.Value is not SendToNodeRedResultDto castedResult)
                throw new InvalidOperationException($"Job {jobId} has status {status} and a result of type {result.Value.GetType()} instead of {typeof(SendToNodeRedResultDto)}.");
            return new(castedResult);
        }
        public string StartDryRun(
            TransactionImportConfigurationDto configuration,
            string accountId,
            List<List<string>> csvData)
        {

            var (jobId, cancellationToken) = jobRegistry.RegisterJob("parsing csv...");
            var importTag = $"import-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            _ = Task.Run(async () =>
            {
                var result = new SendToNodeRedResultDto
                {
                    ImportAccount = new SendToNodeRedResultAccountDataDto
                    {
                        Id = accountId,
                        Name = (await accountStorage.TryGet(accountId.SeedStorageKey<AccountDto>()))
                            .As(a => a.Name)
                            .Or(SpendLessConstants.FallbackAccountName)
                    }
                };
                var results = new List<(SendToNodeRedRequestDto Request, SendToNodeRedResponseDto Response)>();

                if (csvData.Count == 0)
                {
                    jobRegistry.CompleteJob(jobId, result);
                    return;
                }

                var existingCategories = (await categoryStorage.GetAll())
                    .Select(kvp => kvp.Key.SingleValue())
                    .ToHashSet();
                var existingTags = (await tagStorage.GetAll())
                    .Select(kvp => kvp.Key.SingleValue())
                    .ToHashSet();


                try
                {
                    var batchSize = 10; // todo: appsettings
                    //var header = csvData.First();
                    var batches = csvData.Chunk(batchSize);

                    foreach (var batch in batches)
                    {
                        var payloads = batch.Select(b => new SendToNodeRedRequestDto
                        {
                            Account = accountId,
                            Configuration = configuration,
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
                                    SourceRequestPayload = p.ToString()
                                };
                            }
                        }));
                        results.AddRange(batchResults);
                        jobRegistry.UpdateJobProgress(jobId, (double)results.Count / (double)csvData.Count);
                    }

                    jobRegistry.UpdateJobProgress(jobId, (double)results.Count / (double)csvData.Count);

                    foreach (var (request, response) in results)
                    {
                        var resultDto = new SendToNodeRedSingleResultDto
                        {
                            SourceRequestPayload = request.ToString(),
                        };

                        // given an existing id, fetch the name
                        if (response.Transaction.Source.Id != null &&
                            (await accountStorage.TryGet(response.Transaction.Source.Id.SeedStorageKey<AccountDto>())).Test(out var account))
                        {
                            response.Transaction.Source.Name = account.Name;
                        }
                        // given a new id, either grab the name from the new accounts
                        // or default it to the fallback name and add it to the new accounts
                        else if (response.Transaction.Source.Id != null)
                        {
                            if (result.NewAccounts.TryGetValue(response.Transaction.Source.Id, out var name))
                                response.Transaction.Source.Name = name;
                            else
                                result.NewAccounts[response.Transaction.Source.Id] = response.Transaction.Source.Name ??= SpendLessConstants.FallbackAccountName;
                        }
                        // given a name sans id, try and find the id based on the name in the new accounts
                        // otherwise generate a new id and add it to the new accounts
                        // also warn if we are going to create an account with the same name as an existing one
                        else if (response.Transaction.Source.Name != null)
                        {
                            foreach (var (key, value) in result.NewAccounts)
                                if (value == response.Transaction.Source.Name)
                                {
                                    response.Transaction.Source.Id = key;
                                    break;
                                }
                            if (response.Transaction.Source.Id == null)
                            {
                                response.Transaction.Source.Id = Guid.NewGuid().ToString();
                                result.NewAccounts[response.Transaction.Source.Id] = response.Transaction.Source.Name;
                                if (await accountStorage.ContainsForeignKey(AccountDto.GetNameForeignKey(
                                    response.Transaction.Source.Name)))
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
                            (await accountStorage.TryGet(response.Transaction.Destination.Id.SeedStorageKey<AccountDto>())).Test(out account))
                        {
                            response.Transaction.Destination.Name = account.Name;
                        }
                        // given a new id, either grab the name from the new accounts
                        // or default it to the fallback name and add it to the new accounts
                        else if (response.Transaction.Destination.Id != null)
                        {
                            if (result.NewAccounts.TryGetValue(response.Transaction.Destination.Id, out var name))
                                response.Transaction.Destination.Name = name;
                            else
                                result.NewAccounts[response.Transaction.Destination.Id] = response.Transaction.Destination.Name ??= SpendLessConstants.FallbackAccountName;
                        }
                        // given a name sans id, try and find the id based on the name in the new accounts
                        // otherwise generate a new id and add it to the new accounts
                        // also warn if we are going to create an account with the same name as an existing one
                        else if (response.Transaction.Destination.Name != null)
                        {
                            foreach (var (key, value) in result.NewAccounts)
                                if (value == response.Transaction.Destination.Name)
                                {
                                    response.Transaction.Destination.Id = key;
                                    break;
                                }
                            if (response.Transaction.Destination.Id == null)
                            {
                                response.Transaction.Destination.Id = Guid.NewGuid().ToString();
                                result.NewAccounts[response.Transaction.Destination.Id] = response.Transaction.Destination.Name;
                                if (await accountStorage.ContainsForeignKey(AccountDto.GetNameForeignKey(
                                    response.Transaction.Destination.Name)))
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


                        resultDto.Transaction = new SendToNodeRedTransactionResultDto
                        {
                            Amount = response.Transaction.Amount,
                            Category = response.Transaction.Category,
                            Tags = response.Transaction.Tags,
                            Source = new SendToNodeRedResultAccountDataDto
                            {
                                Id = response.Transaction.Source.Id,
                                Name = response.Transaction.Source.Name
                            },
                            Destination = new SendToNodeRedResultAccountDataDto
                            {
                                Id = response.Transaction.Destination.Id,
                                Name = response.Transaction.Destination.Name
                            },
                            Description = response.Transaction.Description
                        };

                        if (configuration.AddImportTag)
                            resultDto.Transaction.Value.Tags.Add(importTag);

                        result.Transactions.Add(resultDto);

                        if (resultDto.Transaction.Value.Source.Id == accountId)
                            result.BalanceChange -= resultDto.Transaction.Value.Amount;
                        else if (resultDto.Transaction.Value.Destination.Id == accountId)
                            result.BalanceChange += resultDto.Transaction.Value.Amount;

                        foreach (var tag in resultDto.Transaction.Value.Tags)
                        {
                            if (existingTags.Contains(tag))
                                continue;
                            if (result.NewTags.ContainsKey(tag))
                                result.NewTags[tag] += 1;
                            else
                                result.NewTags[tag] = 1;
                        }

                        if (resultDto.Transaction.Value.Category == SpendLessConstants.DefaultCategory)
                            resultDto.Warnings.Add(TransactionImportWarning.MissingCategory);
                        else if (!existingCategories.Contains(resultDto.Transaction.Value.Category))
                        {
                            if (result.NewCategories.ContainsKey(resultDto.Transaction.Value.Category))
                                result.NewCategories[resultDto.Transaction.Value.Category] += 1;
                            else
                                result.NewCategories[resultDto.Transaction.Value.Category] = 1;
                        }

                    }

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (SendToNodeRedException ex)
                {
                    result.Transactions = new List<SendToNodeRedSingleResultDto>
                    {
                        new SendToNodeRedSingleResultDto
                        {
                            SourceRequestPayload = ex.SourceRequestPayload,
                            Errors = new HashSet<string>
                            {
                                ex.Message
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
    }
}
