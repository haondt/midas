﻿using Haondt.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Admin.Models;
using Midas.Domain.Admin.Services;
using Midas.Domain.Dashboard.Services;
using Midas.Domain.Import.Services;
using Midas.Domain.Kvs.Services;
using Midas.Domain.Merge.Services;
using Midas.Domain.NodeRed.Models;
using Midas.Domain.NodeRed.Services;
using Midas.Domain.Reconcile.Services;
using Midas.Domain.Reporting.Models;
using Midas.Domain.Reporting.Services;
using Midas.Domain.Shared.Services;
using Midas.Domain.Split.Services;
using Midas.Domain.Supercategories.Services;
using Midas.Domain.Transactions.Services;
using Polly;
using Polly.Timeout;

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
            services.Configure<TakeoutSettings>(configuration.GetSection(nameof(TakeoutSettings)));

            services.Configure<NodeRedSettings>(configuration.GetSection(nameof(NodeRedSettings)));
            services.AddHttpClient<INodeRedService, NodeRedService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<NodeRedSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
            }).AddPolicyHandler(GetNodeRedPolicy(configuration));
            services.AddSingleton<IKvsService, KvsService>();
            services.AddSingleton<IDashboardService, DashboardService>();
            services.AddSingleton<IMergeService, MergeService>();
            services.AddSingleton<IAccountsService, AccountsService>();
            services.AddSingleton<ISupercategoryService, SupercategoryService>();
            services.AddSingleton<ISplitService, SplitService>();
            return services;
        }

        private static AsyncTimeoutPolicy<HttpResponseMessage> GetNodeRedPolicy(IConfiguration configuration)
        {
            var settings = configuration.GetRequiredSection<NodeRedSettings>();
            var logger = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            }).CreateLogger<Policy>();
            var timeoutPolicy = Policy
                .TimeoutAsync<HttpResponseMessage>(settings.TimeoutSeconds, (ct, ts, t) =>
                {
                    logger.LogInformation("Timed out NodeRed request.");
                    return Task.CompletedTask;
                });
            return timeoutPolicy;
        }
    }
}
