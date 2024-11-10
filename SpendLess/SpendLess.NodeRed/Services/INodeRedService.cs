
namespace SpendLess.NodeRed.Services
{
    public interface INodeRedService
    {
        Task<string> SendToNodeRed(string input, CancellationToken? cancellationToken = null);
    }
}