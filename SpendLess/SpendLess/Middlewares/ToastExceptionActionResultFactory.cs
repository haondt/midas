using Haondt.Web.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.Services;
using SpendLess.Components.SpendLessComponents;
using SpendLess.Core.Exceptions;

namespace SpendLess.Middlewares
{
    public class ToastExceptionActionResultFactory(ISingletonComponentFactory componentFactoryFactory) : ISpecificExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception)
        {
            return true;
        }

        public async Task<IActionResult> CreateAsync(Exception exception, HttpContext context)
        {
            var severity = ToastSeverity.Error;
            var model = new ToastModel { Message = $"{exception.GetType().Name}: {exception.Message}", Severity = severity };
            if (exception is UserException)
            {
                model.Message = exception.Message;
                model.Severity = ToastSeverity.Warning;
            }
            var componentFactory = componentFactoryFactory.CreateComponentFactory();

            var errorComponent = await componentFactory.GetPlainComponent(model, configureResponse: m => m.SetStatusCode = 500);


            return Components.Extensions.ComponentExtensions.CreateView(errorComponent, context.Response.AsResponseData());
        }
    }
}
