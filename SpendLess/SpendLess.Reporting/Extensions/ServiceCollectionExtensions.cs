using SpendLess.Reporting.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Dashboard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReporting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationSection
            {
                Label = "Reporting",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "Generate Report",
                        Slug = "reports",
                    }
                }
            });

            services.AddSingleton<IReportService, ReportService>();

            return services;
        }
    }
}
