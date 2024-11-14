namespace SpendLess.Domain.Models
{
    public class SendToNodeRedResultDto
    {
        public List<(string Message, string SourceRequestPayload)> Errors { get; set; } = [];

    }
}
