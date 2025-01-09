using Haondt.Web.Core.Services;

namespace Midas.Middlewares
{
    public interface ISpecificExceptionActionResultFactory : IExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception);
    }
}
