
using SpendLess.NodeRed.Models;

namespace SpendLess.NodeRed.Services
{
    public interface INodeRedService
    {
        Task<SendToNodeRedResponseDto> SendToNodeRed(SendToNodeRedRequestDto input, CancellationToken? cancellationToken = null);
        Task<string> SendToNodeRed(string input, CancellationToken? cancellationToken = null);
    }
}