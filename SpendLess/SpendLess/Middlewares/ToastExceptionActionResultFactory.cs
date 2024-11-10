using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.Services;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;

namespace SpendLess.Middlewares
{
    public class ToastExceptionActionResultFactory(ISingletonComponentFactory componentFactoryFactory) : IExceptionActionResultFactory
    {
        public async Task<IActionResult> CreateAsync(Exception exception, HttpContext context)
        {
            var severity = ToastSeverity.Error;
            if (exception is UserException)
                severity = ToastSeverity.Warning;
            var model = new ToastModel { Message = $"{exception.GetType().Name}: {exception.Message}", Severity = severity };
            var componentFactory = componentFactoryFactory.CreateComponentFactory();

            var errorComponent = await componentFactory.GetPlainComponent(model, configureResponse: m => m.SetStatusCode = 500);


            return Components.Extensions.ComponentExtensions.CreateView(errorComponent, context.Response.AsResponseData());
        }
    }
}
