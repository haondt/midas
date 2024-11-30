using Haondt.Core.Models;
using SpendLess.Domain.Models;
using SpendLess.Domain.Services;
using SpendLess.NodeRed.Models;
using SpendLess.NodeRed.Services;
using SpendLess.TransactionImport.Exceptions;

namespace SpendLess.TransactionImport.Services
{
    public class TransactionImportService(IAsyncJobRegistry jobRegistry,
        INodeRedService nodeRed) : ITransactionImportService
    {

        public Result<SendToNodeRedResultDto, double> GetDryRunResult(string jobId)
        {
            var (status, progress) = jobRegistry.GetJobProgress(jobId);
            if (status < AsyncJobStatus.Complete)
                return new(progress * 100);
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

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();

            _ = Task.Run(async () =>
            {
                var result = new SendToNodeRedResultDto();
                var results = new List<SendToNodeRedResponseDto>();

                if (csvData.Count == 0)
                {
                    jobRegistry.CompleteJob(jobId, result);
                    return;
                }

                try
                {
                    var batchSize = 50; // todo: appsettings
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
                                return await nodeRed.SendToNodeRed(p);
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

                    jobRegistry.CompleteJob(jobId, result);
                }
                catch (SendToNodeRedException ex)
                {
                    result.Errors.Add((ex.Message, ex.SourceRequestPayload));
                    jobRegistry.FailJob(jobId, result);
                    throw;
                }
                catch (Exception)
                {
                    jobRegistry.FailJob(jobId);
                    throw;
                }
            }, cancellationToken);

            return jobId;
        }
    }
}
