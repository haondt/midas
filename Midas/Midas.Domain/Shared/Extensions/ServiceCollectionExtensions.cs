using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Admin.Models;
using Midas.Domain.Admin.Services;
using Midas.Domain.Dashboard.Services;
using Midas.Domain.Import.Services;
using Midas.Domain.Kvs.Services;
using Midas.Domain.NodeRed.Models;
using Midas.Domain.NodeRed.Services;
using Midas.Domain.Reconcile.Services;
using Midas.Domain.Reporting.Models;
using Midas.Domain.Reporting.Services;
using Midas.Domain.Shared.Services;
using Midas.Domain.Transactions.Services;

namespace Midas.Domain.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMidasDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAsyncJobRegistry, AsyncJobRegistry>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<ITransactionImportService, TransactionImportService>();
            services.AddSingleton<IReconcileService, ReconcileService>();
            services.AddSingleton<IReportService, ReportService>();
            services.Configure<ReportingSettings>(configuration.GetSection(nameof(ReportingSettings)));


            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IFileService, FileService>();
            services.Configure<FileSettings>(configuration.GetSection(nameof(FileSettings)));

            services.Configure<NodeRedSettings>(configuration.GetSection(nameof(NodeRedSettings)));
            services.AddHttpClient<INodeRedService, NodeRedService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<NodeRedSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
            }).AddPolicyHandler(GetNodeRedPolicy());
            services.AddSingleton<IKvsService, KvsService>();
            services.AddSingleton<IDashboardService, DashboardService>();

            services.AddSingleton<IAccountsService, AccountsService>();
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
