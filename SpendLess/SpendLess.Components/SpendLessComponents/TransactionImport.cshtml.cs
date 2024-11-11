using Haondt.Web.Core.Components;
using SpendLess.Components.Abstractions;

namespace SpendLess.Components.SpendLessComponents
{
    public class TransactionImportModel : IComponentModel
    {
    }

    public class TransactionImportComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<TransactionImportModel>(() => new())
            {
                ViewPath = $"~/SpendLessComponents/TransactionImport.cshtml"
            };
        }
    }

}
