using Newtonsoft.Json;
using SpendLess.NodeRed.Exceptions;
using SpendLess.NodeRed.Models;
using System.Text;

namespace SpendLess.NodeRed.Services
{
    public class NodeRedService(HttpClient httpClient) : INodeRedService
    {
        private const string ApplyPath = "/apply";
        //private const string ExtractKeyPath = "/extract-key";

        public static JsonSerializerSettings SerializerSettings { get; set; }
        static NodeRedService()
        {
            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.TypeNameHandling = TypeNameHandling.None;
            SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
            SerializerSettings.Formatting = Formatting.Indented;
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

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
            var str = JsonConvert.SerializeObject(input);
            var result = await SendToNodeRed(str, cancellationToken);
            return JsonConvert.DeserializeObject<SendToNodeRedResponseDto>(result)
                ?? throw new NodeRedException($"failed to deserialize response to {typeof(SendToNodeRedResponseDto)}: {result}");
        }

        //public async Task<Result<string, string>> ExtractKey(string field, string input, CancellationToken? cancellationToken)
        //{
        //    var response = await SendToNodeRed($"{ExtractKeyPath}/{field}", input, cancellationToken);

        //    var result = await response.EnsureDownstreamSuccessStatusCode("Node-Red");
        //    if (!result.IsSuccessful)
        //        return ResultExtensions.Fail<string, string>(result.Reason);
        //    var content = await response.Content.ReadAsStringAsync();
        //    return ResultExtensions.Success<string, string>(content);
        //}

        //public async Task<Result<Result<string, ApplyRulesReason>, string>> ApplyRules(string input, CancellationToken? cancellationToken)
        //{
        //    var response = await SendToNodeRed(ApplyPath, input, cancellationToken);
        //    if (response.StatusCode == HttpStatusCode.OK)
        //        return new(new Result<string, ApplyRulesReason>(await response.Content.ReadAsStringAsync()));
        //    if (response.StatusCode == HttpStatusCode.NotModified)
        //        return new(new Result<string, ApplyRulesReason>(ApplyRulesReason.NotModified));

        //    var str = await (response.Content?.ReadAsStringAsync() ?? Task.FromResult("null"));
        //    return new($"Node-Red returned status {response.StatusCode} with content: {str}");
        //}
    }
}
