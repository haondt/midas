namespace Midas.UI.Shared.Exceptions
{
    public class NotFoundPageException : PageException
    {
        public NotFoundPageException()
        {
        }

        public NotFoundPageException(string? message) : base(message)
        {
        }

        public NotFoundPageException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public override string Title => "Not Found";
        public override int Code => 404;
    }
}
