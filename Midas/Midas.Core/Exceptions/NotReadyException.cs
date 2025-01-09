namespace Midas.Core.Exceptions
{
    public class NotReadyException : Exception
    {
        public NotReadyException() : base() { }
        public NotReadyException(string message) : base(message) { }
        public NotReadyException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
