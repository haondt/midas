using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Domain.Models;
using SpendLess.Domain.Services;
using SpendLess.Web.Domain.Services;
using SpendLess.Web.Domain.SpendLess.Domain;

namespace SpendLess.EventHandlers.TransactionImport
{
    public class TransactionImportCheckProgressEventHandler(
        IComponentFactory componentFactory,
        IAsyncJobRegistry jobRegistry) : ISingleEventHandler
    {
        public string EventName => "TransactionImportCheckProgress";

        public async Task<IComponent> HandleAsync(IRequestData requestData)
        {
            var id = requestData.Form.GetValue<string>("job");
            var (status, progress) = jobRegistry.GetJobProgress(id);

            if (status >= AsyncJobStatus.Complete)
            {
                var result = jobRegistry.GetJobResult(id);
                if (!result.HasValue || result.Value is not SendToNodeRedResultDto resultDto)
                {
                    return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
                    {
                        Components = new List<IComponent>
                        {
                            await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
                            {
                                DryRunProgress = 100,
                            }),
                            await componentFactory.GetPlainComponent(new ToastModel
                            {
                                Message = status > AsyncJobStatus.Complete
                                    ? "Transaction import failed for an unknown reason."
                                    : "Transaction import completed but unable to parse result.",
                                Severity = ToastSeverity.Error
                            })
                        }
                    });
                }
                return await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
                {
                    DryRunProgress = 100,
                    DryRunResult = resultDto
                });
            }
            return await componentFactory.GetPlainComponent(new TransactionImportUpdateModel
            {
                DryRunProgress = progress * 100,
                DryRunJobId = id
            });
        }
    }
}
