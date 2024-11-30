namespace SpendLess.TransactionImport.Exceptions
{
    public class SendToNodeRedException : Exception
    {
        public required string SourceRequestPayload { get; set; }

        public SendToNodeRedException()
        {
        }

        public SendToNodeRedException(string? message) : base(message)
        {
        }

        public SendToNodeRedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
