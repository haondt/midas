namespace SpendLess.Core.Exceptions
{
    public class DownstreamException : Exception
    {
        public DownstreamException() : base() { }
        public DownstreamException(string message) : base(message) { }
        public DownstreamException(string? message, Exception? innerException) : base(message, innerException) { }

    }
}
