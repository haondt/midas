using Haondt.Web.Core.Services;
using SpendLess.Middlewares;

namespace SpendLess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessServices(this IServiceCollection services, IConfiguration configuration)
        {

            // middleware
            services.AddSingleton<ISpecificExceptionActionResultFactory, PageExceptionActionResultFactory>();
            services.AddSingleton<ISpecificExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddSingleton<IExceptionActionResultFactory, ExceptionActionResultFactoryDelegator>();

            return services;
        }
    }
}
