using SpendLess.Domain.Models;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportSendToNodeRedResultModel
    {
        public required SendToNodeRedResultDto Inner { get; set; }

    }
    //public class TransactionImportSendToNodeRedResultComponentDescriptorFactory : IComponentDescriptorFactory
    //{
    //    public const string ViewPath = $"~/SpendLessComponents/TransactionImportSendToNodeRedResult.cshtml";
    //    public IComponentDescriptor Create()
    //    {
    //        return new ComponentDescriptor<TransactionImportSendToNodeRedResultModel>()
    //        {
    //            ViewPath = ViewPath,
    //        };
    //    }
    //}
}
