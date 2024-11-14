using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Models;
using SpendLess.Domain.Services;
using SpendLess.Exceptions;
using SpendLess.Extensions;
using SpendLess.NodeRed.Models;
using SpendLess.NodeRed.Services;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;

namespace SpendLess.EventHandlers.TransactionImport
{
    public class TransactionImportDryRunEventHandler(IComponentFactory componentFactory,
        IAsyncJobRegistry jobRegistry,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configurationStorage,
        INodeRedService nodeRed) : ISingleEventHandler
    {
        public string EventName => "TransactionImportDryRun";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var file = (requestData.Form.Files?.FirstOrDefault(f => f.Name == "file"))
                ?? throw new UserException("Please select a file.");

            var accountId = requestData.Form.GetValue<string>("account");
            var configurationId = requestData.Form.GetValue<string>("config-id");
            var configuration = await configurationStorage.Get(configurationId.SeedStorageKey<TransactionImportConfigurationDto>());

            var csvData = file.ParseAsCsv();

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {
                var result = new SendToNodeRedResultDto();

                if (csvData.Count == 0)
                {
                    jobRegistry.CompleteJob(jobId, result);
                    return;
                }

                try
                {
                    var batchSize = 50; // todo: appsettings
                    var header = csvData.First();
                    var batches = csvData.Chunk(batchSize);

                    foreach (var batch in batches)
                    {
                        var payloads = batch.Select(b => new SendToNodeRedRequestDto
                        {
                            Account = accountId,
                            Configuration = configuration,
                            Csv = new CsvData
                            {
                                FirstRow = header,
                                Row = b
                            }
                        });

                        var results = await Task.WhenAll(payloads.Select(async p =>
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
                        //result.AddRange(results);
                        //jobRegistry.UpdateJobProgress(jobId, result.Count / csvData.Count);
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

            return await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
            {
                DryRunProgress = 0,
                DryRunJobId = jobId
            });
        }
    }
}
