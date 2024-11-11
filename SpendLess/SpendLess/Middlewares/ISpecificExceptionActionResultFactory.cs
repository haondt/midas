using Haondt.Web.Core.Services;

namespace SpendLess.Middlewares
{
    public interface ISpecificExceptionActionResultFactory : IExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception);
    }
}
