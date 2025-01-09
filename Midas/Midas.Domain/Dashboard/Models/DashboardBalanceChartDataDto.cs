using Midas.Core.Models;

namespace Midas.Domain.Dashboard.Models
{
    public class DashboardBalanceChartDataDto
    {
        public required List<string> AccountNames { get; set; }
        public required List<AbsoluteDateTime> TimeStamps { get; set; }
        public required List<List<decimal>> Balances { get; set; }
    }
}
