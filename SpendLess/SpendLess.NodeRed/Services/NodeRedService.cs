using Haondt.Core.Models;
using Newtonsoft.Json;
using SpendLess.Domain.Constants;
using SpendLess.NodeRed.Exceptions;
using SpendLess.NodeRed.Models;
using System.Text;

namespace SpendLess.NodeRed.Services
{
    public class NodeRedService(HttpClient httpClient) : INodeRedService
    {
        private const string ApplyPath = "/apply";
        private const string ExportPath = "/export";

        private async Task<HttpResponseMessage> InternalSendToNodeRed(string path, string input, CancellationToken? cancellationToken = null)
        {
            var content = new StringContent(input, Encoding.UTF8, "application/json");
            var response = cancellationToken.HasValue
                ? await httpClient.PostAsync(path, content, cancellationToken.Value)
                : await httpClient.PostAsync(path, content);
            return response;
        }

        public async Task<string> SendToNodeRed(string input, CancellationToken? cancellationToken = null)
        {

            var response = await InternalSendToNodeRed(ApplyPath, input);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<SendToNodeRedResponseDto> SendToNodeRed(SendToNodeRedRequestDto input, CancellationToken? cancellationToken = null)
        {
            var result = await SendToNodeRed(input.ToString(), cancellationToken);
            return JsonConvert.DeserializeObject<SendToNodeRedResponseDto>(result, SpendLessConstants.ApiSerializerSettings)
                ?? throw new NodeRedException($"failed to deserialize response to {typeof(SendToNodeRedResponseDto)}: {result}");
        }

        public async Task<NodeRedExportDto> ExportDataAsync()
        {
            var response = await httpClient.GetAsync(ExportPath);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NodeRedExportDto>(responseString, SpendLessConstants.ApiSerializerSettings);
            return result
                ?? throw new NodeRedException($"failed to deserialize response to {typeof(NodeRedExportDto)}: {result}");
        }

        public async Task<(int ResponseCode, Optional<string> Body)> SendToNodeRedRaw(string input, CancellationToken? cancellationToken = null)
        {
            var response = await InternalSendToNodeRed(ApplyPath, input);
            var status = (int)response.StatusCode;
            if (response.Content != null)
                return (status, await response.Content.ReadAsStringAsync());
            return (status, new());
        }

    }
}
