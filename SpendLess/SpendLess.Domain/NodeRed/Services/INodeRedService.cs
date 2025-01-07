
using Haondt.Core.Models;
using SpendLess.Domain.NodeRed.Models;

namespace SpendLess.Domain.NodeRed.Services
{
    public interface INodeRedService
    {
        Task<NodeRedExportDto> ExportDataAsync();
        Task<Optional<SendToNodeRedResponseDto>> SendToNodeRed(SendToNodeRedRequestDto input, CancellationToken? cancellationToken = null);
        Task<string> SendToNodeRed(string input, CancellationToken? cancellationToken = null);
        Task<(int ResponseCode, Optional<string> Body)> SendToNodeRedRaw(string input, CancellationToken? cancellationToken = null);
    }
}