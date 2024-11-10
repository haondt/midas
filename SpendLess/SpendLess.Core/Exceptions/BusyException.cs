namespace SpendLess.Core.Exceptions
{
    public class BusyException : Exception
    {
        public BusyException() : base() { }
        public BusyException(string message) : base(message) { }
        public BusyException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
