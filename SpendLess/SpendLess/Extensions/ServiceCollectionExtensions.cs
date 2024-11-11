using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using SpendLess.EventHandlers;
using SpendLess.EventHandlers.Accounts;
using SpendLess.EventHandlers.NodeRed;
using SpendLess.Middlewares;

namespace SpendLess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // event handlers
            services.AddScoped<IEventHandler, SingleEventHandlerRegistry>();

            services.AddScoped<ISingleEventHandler, PrettifyRequestEventHandler>();
            services.AddScoped<ISingleEventHandler, SendToNodeRedEventHandler>();
            services.AddScoped<ISingleEventHandler, LoadDefaultNodeRedPalyoadEventHandler>();

            services.AddScoped<ISingleEventHandler, AccountUpsertEventHandler>();

            // middleware
            services.AddSingleton<ISpecificExceptionActionResultFactory, PageExceptionActionResultFactory>();
            services.AddSingleton<ISpecificExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddSingleton<IExceptionActionResultFactory, ExceptionActionResultFactoryDelegator>();

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
                    ],
                    ""scrollIntoViewOnBoost"": false
                }",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = "https://unpkg.com/htmx-ext-multi-swap@2.0.0/multi-swap.js"
            });
            return services;
        }

    }
}
