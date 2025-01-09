using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Midas.UI.Shared.Components;
using Midas.UI.Shared.Models;

namespace Midas.UI.Shared.Middlewares
{
    public class ModelStateValidationFilter(IComponentFactory componentFactory) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ModelState.IsValid)
            {
                await next();
                return;
            }

            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var text = string.Join('\n', errors);

            var result = await componentFactory.RenderComponentAsync(new Toast
            {
                Message = text,
                Severity = ToastSeverity.Error
            });

            context.HttpContext.Response.AsResponseData()
                .Status(400)
                .HxReswap("none");

            await result.ExecuteAsync(context.HttpContext);
        }
    }
}
