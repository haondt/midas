using Haondt.Web.Core.Services;
using Midas.Middlewares;

namespace Midas.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMidasServices(this IServiceCollection services, IConfiguration configuration)
        {

            // middleware
            services.AddSingleton<ISpecificExceptionActionResultFactory, PageExceptionActionResultFactory>();
            services.AddSingleton<ISpecificExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddSingleton<IExceptionActionResultFactory, ExceptionActionResultFactoryDelegator>();

            return services;
        }
    }
}
