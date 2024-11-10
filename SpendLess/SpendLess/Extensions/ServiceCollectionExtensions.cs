using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using SpendLess.EventHandlers;
using SpendLess.Middlewares;

namespace SpendLess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // event handlers
            services.AddScoped<IEventHandler, SingleEventHandlerRegistry>();

            services.AddScoped<ISingleEventHandler, NavigateEventHandler>();

            // middleware
            services.AddSingleton<IExceptionActionResultFactory, ToastExceptionActionResultFactory>();

            return services;
        }

        public static IServiceCollection AddSpendLessHeadEntries(this IServiceCollection services)
        {
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://kit.fontawesome.com/afd44816da.js",
                CrossOrigin = "anonymous"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new MetaDescriptor
            {
                Name = "htmx-config",
                Content = @"{
                    ""responseHandling"": [
                        { ""code"": ""204"", ""swap"": false },
                        { ""code"": "".*"", ""swap"": true }
                    ]
                }",
            });
            return services;
        }

    }
}
