using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Components.Services;
using SpendLess.Core.Exceptions;
using SpendLess.Web.Core.Extensions;
using SpendLess.Web.Domain.SpendLess.Domain;

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
            var statusCode = 500;
            var model = new ToastModel { Message = $"{exception.GetType().Name}: {exception.Message}", Severity = severity };
            if (exception is UserException)
            {
                model.Message = exception.Message;
                model.Severity = ToastSeverity.Warning;
                statusCode = 400;
            }
            var componentFactory = componentFactoryFactory.CreateComponentFactory();

            var errorComponent = await componentFactory.GetPlainComponent(model,
                configureResponse: m =>
                {
                    m.SetStatusCode = statusCode;
                    m.ConfigureHeadersAction = new HxHeaderBuilder()
                        .ReSwap("none")
                        .Build();
                });
            return errorComponent.CreateView(context.Response.AsResponseData());
        }
    }
}
