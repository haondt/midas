namespace SpendLess.NodeRed.Exceptions
{
    public class NodeRedException : Exception
    {
        public NodeRedException()
        {
        }

        public NodeRedException(string? message) : base(message)
        {
        }

        public NodeRedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
