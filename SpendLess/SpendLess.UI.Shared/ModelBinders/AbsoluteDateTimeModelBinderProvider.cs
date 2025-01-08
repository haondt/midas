using Microsoft.AspNetCore.Mvc.ModelBinding;
using SpendLess.Core.Models;

namespace SpendLess.UI.Shared.ModelBinders
{
    public class AbsoluteDateTimeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(AbsoluteDateTime) || Nullable.GetUnderlyingType(context.Metadata.ModelType) == typeof(AbsoluteDateTime))
                return new AbsoluteDateTimeModelBinder();
            return null;
        }
    }
}
