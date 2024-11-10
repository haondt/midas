using FireflyIIIpp.NodeRed.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using SpendLess.NodeRed.Services;

namespace SpendLess.NodeRed.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNodeRedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NodeRedSettings>(configuration.GetSection(nameof(NodeRedSettings)));
            services.AddHttpClient<INodeRedService, NodeRedService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<NodeRedSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
            }).AddPolicyHandler(GetNodeRedPolicy());

            return services;
        }

        private static AsyncTimeoutPolicy<HttpResponseMessage> GetNodeRedPolicy()
        {
            var logger = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            }).CreateLogger<Policy>();
            var timeoutPolicy = Policy
                .TimeoutAsync<HttpResponseMessage>(1, (ct, ts, t) =>
                {
                    logger.LogInformation("Timed out NodeRed request.");
                    return Task.CompletedTask;
                });
            return timeoutPolicy;
        }
    }
}
