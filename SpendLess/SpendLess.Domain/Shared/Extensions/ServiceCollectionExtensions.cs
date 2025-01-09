using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using SpendLess.Domain.Accounts.Services;
using SpendLess.Domain.Admin.Models;
using SpendLess.Domain.Admin.Services;
using SpendLess.Domain.Dashboard.Services;
using SpendLess.Domain.Import.Services;
using SpendLess.Domain.Kvs.Services;
using SpendLess.Domain.NodeRed.Models;
using SpendLess.Domain.NodeRed.Services;
using SpendLess.Domain.Reconcile.Services;
using SpendLess.Domain.Reporting.Models;
using SpendLess.Domain.Reporting.Services;
using SpendLess.Domain.Shared.Services;
using SpendLess.Domain.Transactions.Services;

namespace SpendLess.Domain.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpendLessDomainServices(this IServiceCollection services, IConfiguration configuration)
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
