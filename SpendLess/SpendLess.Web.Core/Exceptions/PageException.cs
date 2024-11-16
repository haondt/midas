using Haondt.Core.Models;

namespace SpendLess.Web.Core.Exceptions
{
    public abstract class PageException : Exception
    {
        public virtual int Code { get; } = 500;
        public abstract string Title { get; }
        public Optional<string> MessageArgument { get; } = new();

        public PageException()
        {
        }

        public PageException(string? message) : base(message)
        {
            if (!string.IsNullOrEmpty(message))
                MessageArgument = new(message);
        }

        public PageException(string? message, Exception? innerException) : base(message, innerException)
        {
            if (!string.IsNullOrEmpty(message))
                MessageArgument = new(message);
        }

    }
}
