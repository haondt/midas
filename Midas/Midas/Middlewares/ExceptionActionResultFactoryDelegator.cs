using Haondt.Web.Core.Services;

namespace Midas.Middlewares
{
    public class ExceptionActionResultFactoryDelegator(IEnumerable<ISpecificExceptionActionResultFactory> factories) : IExceptionActionResultFactory
    {
        public Task<IResult> CreateAsync(Exception exception, HttpContext context)
        {
            foreach (var factory in factories)
                if (factory.CanHandle(exception))
                    return factory.CreateAsync(exception, context);
            throw new NotSupportedException($"Cannot handle exception of type {exception.GetType()}");
        }
    }
}
