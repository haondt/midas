using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using SpendLess.Domain.Services;

namespace SpendLess.EventHandlers.TransactionImport
{
    public class TransactionImportCheckProgressEventHandler(
        IComponentFactory componentFactory,
        ILogger<TransactionImportCheckProgressEventHandler> logger,
        IAsyncJobRegistry jobRegistry)
    {
        public string EventName => "TransactionImportCheckProgress";

        public async Task<IResult> HandleAsync(IRequestData requestData)
        {
            //var id = requestData.Form.GetValue<string>("job");
            //var (status, progress) = jobRegistry.GetJobProgress(id);

            //if (status >= AsyncJobStatus.Complete)
            //{
            //    var result = jobRegistry.GetJobResult(id);
            //    if (!result.HasValue || result.Value is not SendToNodeRedResultDto resultDto)
            //    {
            //        return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            //        {
            //            Components = new List<IComponent>
            //            {
            //                await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
            //                {
            //                    DryRunProgress = 100,
            //                }),
            //                await componentFactory.GetPlainComponent(new ToastModel
            //                {
            //                    Message = status > AsyncJobStatus.Complete
            //                        ? "Transaction import failed for an unknown reason."
            //                        : "Transaction import completed but unable to parse result.",
            //                    Severity = ToastSeverity.Error
            //                })
            //            }
            //        });
            //    }
            //    return await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
            //    {
            //        DryRunProgress = 100,
            //        DryRunResult = resultDto
            //    });
            //}
            //return await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
            //{
            //    DryRunProgress = progress * 100,
            //    DryRunJobId = id
            //});

            throw new NotImplementedException();
        }
    }
}
