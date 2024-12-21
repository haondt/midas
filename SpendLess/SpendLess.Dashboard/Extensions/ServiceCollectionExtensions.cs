using SpendLess.Dashboard.Services;
using SpendLess.Web.Domain.Models;

namespace SpendLess.Dashboard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDashboard(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationItem
            {
                Title = "Dashboard",
                Slug = "dashboard",
            });

            services.AddSingleton<IDashboardService, DashboardService>();

            return services;
        }
    }
}
