using SpendLess.Domain.Models;

namespace SpendLess.NodeRed.Models
{
    public class SendToNodeRedResponseDto
    {
        public required TransactionDto Transaction { get; set; }
    }
}