﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SpendLess.Web.Core.ModelBinders
{
    public class CheckboxModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
            bindingContext.Result = ModelBindingResult.Success(value == "on" || value == "true");

            return Task.CompletedTask;
        }
    }
}
