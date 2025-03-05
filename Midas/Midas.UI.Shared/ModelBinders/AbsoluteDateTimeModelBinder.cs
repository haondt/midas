using Haondt.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Midas.UI.Shared.ModelBinders
{
    public class AbsoluteDateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = bindingContext.ModelType.IsValueType && Nullable.GetUnderlyingType(bindingContext.ModelType) == null
                    ? ModelBindingResult.Failed()
                    : ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var input = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(input))
            {
                bindingContext.Result = bindingContext.ModelType.IsValueType && Nullable.GetUnderlyingType(bindingContext.ModelType) == null
                    ? ModelBindingResult.Failed()
                    : ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (!DateTime.TryParse(input, out var dt))
            {
                bindingContext.Result = bindingContext.ModelType.IsValueType && Nullable.GetUnderlyingType(bindingContext.ModelType) == null
                    ? ModelBindingResult.Failed()
                    : ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var absoluteTime = dt.Kind switch
            {
                DateTimeKind.Utc
                    or DateTimeKind.Local => AbsoluteDateTime.Create(dt),
                DateTimeKind.Unspecified => AbsoluteDateTime.Create(new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Local)),
                _ => throw new ArgumentOutOfRangeException($"Unkonw {(typeof(DateTimeKind))} {dt.Kind}.")
            };

            bindingContext.Result = ModelBindingResult.Success(absoluteTime);
            return Task.CompletedTask;
        }
    }
}
