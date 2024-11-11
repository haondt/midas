﻿using Haondt.Web.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.Exceptions;
using SpendLess.Components.Services;
using SpendLess.Components.SpendLessComponents;

namespace SpendLess.Middlewares
{
    public class PageExceptionActionResultFactory(ISingletonComponentFactory componentFactoryFactory) : ISpecificExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception)
        {
            return exception is PageException;
        }

        public async Task<IActionResult> CreateAsync(Exception exception, HttpContext context)
        {
            if (exception is not PageException pageException)
                throw new ArgumentException(nameof(exception));

            var result = new ErrorModel
            {
                ErrorCode = pageException.Code,
                Message = pageException.MessageArgument.HasValue ? pageException.MessageArgument.Value : pageException.Title,
                Title = pageException.Title,
                Details = pageException.ToString()
            };

            var componentFactory = componentFactoryFactory.CreateComponentFactory();
            var errorComponent = await componentFactory.GetPlainComponent(result, configureResponse: m => m.SetStatusCode = result.ErrorCode);

            return Components.Extensions.ComponentExtensions.CreateView(errorComponent, context.Response.AsResponseData());
        }
    }
}
