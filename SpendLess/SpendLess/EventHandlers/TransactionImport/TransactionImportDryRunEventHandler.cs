using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;
using SpendLess.Domain.Services;

namespace SpendLess.EventHandlers.TransactionImport
{
    public class TransactionImportDryRunEventHandler(IComponentFactory componentFactory,
        IAsyncJobRegistry jobRegistry) : ISingleEventHandler
    {
        public string EventName => "TransactionImportDryRun";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var file = requestData.Form.Files?.FirstOrDefault(f => f.Name == "file");
            if (file == null)
                throw new UserException("Please select a file.");




            // get the file. In the form the file input has the name "inputFile"

            // the file is a csv

            var (jobId, cancellationToken) = jobRegistry.RegisterJob();
            _ = Task.Run(async () =>
            {

                try
                {

                    // for each row of the csv,
                    // convert the csv into a json array of strings/decimals
                    // print the array
                }
                catch (Exception ex)
                {
                    jobRegistry.FailJob(jobId, $"Task failed with exception \"{ex.Message}.\"");
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
