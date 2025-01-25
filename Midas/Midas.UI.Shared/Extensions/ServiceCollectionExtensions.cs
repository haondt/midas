using Haondt.Web.Assets;
using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Services;
using Midas.UI.Shared.Middlewares;
using Midas.UI.Shared.Models;
using Midas.UI.Shared.Services;

namespace Midas.UI.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddMidasSharedUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILayoutComponentFactory, LayoutComponentFactory>();
            services.AddScoped<ModelStateValidationFilter>();

            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSAssetSources();
            services.AddMidasHeadEntries();

            return services;
        }

        private static IServiceCollection AddMidasHeadEntries(this IServiceCollection services)
        {
            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            var assemblyPrefix = assembly.GetName().Name;

            services.AddSingleton<IAssetSource>(sp => new ManifestAssetSource(assembly));

            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://kit.fontawesome.com/afd44816da.js",
                CrossOrigin = "anonymous"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new MetaDescriptor
            {
                Name = "htmx-config",
                Content = @"{
                    ""responseHandling"": [
                        { ""code"": ""204"", ""swap"": false },
                        { ""code"": "".*"", ""swap"": true }
                    ],
                    ""scrollIntoViewOnBoost"": false
                }",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = $"/_asset/{assemblyPrefix}.wwwroot.hx-rename.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new IconDescriptor
            {
                Uri = $"/_asset/{assemblyPrefix}.wwwroot.favicon.ico"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new TitleDescriptor
            {
                Title = "Midas",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = "https://unpkg.com/htmx-ext-multi-swap@2.0.0/multi-swap.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = "https://unpkg.com/htmx-ext-loading-states@2.0.0/loading-states.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = "https://unpkg.com/htmx-ext-include-vals@2.0.0/include-vals.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new ScriptDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/chart.js@4.4.7/dist/chart.umd.min.js"
            });
            // disable chartjs animations
            services.AddScoped<IHeadEntryDescriptor>(_ => new JavascriptDescriptor
            {
                Body = @"
                    Chart.defaults.animation = false;
                "
            });
            return services;
        }

    }
}
