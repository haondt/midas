using SpendLess.Dashboard.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Dashboard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDashboard(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationSection
            {
                Label = "Reports",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "Dashboard",
                        Slug = "dashboard",
                    }
                }
            });

            services.AddSingleton<IDashboardService, DashboardService>();

            return services;
        }
    }
}
