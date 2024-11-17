using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using SpendLess.Web.Core.Abstractions;

namespace SpendLess.Admin.SpendLess.Admin
{
    public class AdminComponentDescriptorFactory : IComponentDescriptorFactory
    {
        public IComponentDescriptor Create()
        {
            return new ComponentDescriptor<AdminModel>(ComponentFactory)
            {
                ViewPath = $"~/SpendLess.Admin/Admin.cshtml",
            };
        }

        private async Task<(AdminModel, Optional<Action<IHttpResponseMutator>>)> ComponentFactory(IComponentFactory componentFactory, IRequestData requestData)
        {
            return (new(), new());
        }
    }
}
