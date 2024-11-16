using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SpendLess.Web.Core.Extensions
{
    public static class ComponentExtensions
    {
        public static ViewResult CreateView(this IComponent component, IResponseData responseData)
        {
            if (component.ConfigureResponse.HasValue)
            {
                var mutator = new HttpResponseMutator();
                component.ConfigureResponse.Value.Invoke(mutator);
                mutator.Apply(responseData);
            }

            var vdd = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = component.Model
            };

            return new ViewResult
            {
                ViewName = component.ViewPath,
                ViewData = vdd
            };
        }
        public static ViewResult CreateView<T>(this IComponent<T> component, IResponseData responseData) where T : IComponentModel
        {
            if (component.ConfigureResponse.HasValue)
            {
                var mutator = new HttpResponseMutator();
                component.ConfigureResponse.Value.Invoke(mutator);
                mutator.Apply(responseData);
            }

            var vdd = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = component.Model
            };

            return new ViewResult
            {
                ViewName = component.ViewPath,
                ViewData = vdd
            };
        }
    }
}
