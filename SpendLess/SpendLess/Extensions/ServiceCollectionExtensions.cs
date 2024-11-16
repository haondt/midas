using Haondt.Web.Core.Services;
using SpendLess.EventHandlers.Accounts;
using SpendLess.EventHandlers.NodeRed;
using SpendLess.EventHandlers.TransactionImport;
using SpendLess.Middlewares;
using SpendLess.Web.Domain.Services;

namespace SpendLess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // event handlers

            services.AddScoped<ISingleEventHandler, PrettifyRequestEventHandler>();
            services.AddScoped<ISingleEventHandler, SendToNodeRedEventHandler>();
            services.AddScoped<ISingleEventHandler, LoadDefaultNodeRedPayloadEventHandler>();

            services.AddScoped<ISingleEventHandler, AccountUpsertEventHandler>();

            services.AddScoped<ISingleEventHandler, TransactionImportDryRunEventHandler>();
            services.AddScoped<ISingleEventHandler, TransactionImportUpsertConfigurationEventHandler>();
            services.AddScoped<ISingleEventHandler, TransactionImportCheckProgressEventHandler>();

            // middleware
            services.AddSingleton<ISpecificExceptionActionResultFactory, PageExceptionActionResultFactory>();
            services.AddSingleton<ISpecificExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddSingleton<IExceptionActionResultFactory, ExceptionActionResultFactoryDelegator>();

            return services;
        }


    }
}
