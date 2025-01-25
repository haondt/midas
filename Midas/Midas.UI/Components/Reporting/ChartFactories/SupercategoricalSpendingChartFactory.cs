using Midas.Core.Constants;
using Midas.UI.Models.Charts;

namespace Midas.UI.Components.Reporting.ChartFactories
{
    public partial class ReportingChartFactory
    {
        public required List<(string SupercategoryName, decimal Amount)> SupercategoricalSpending { get; set; }

        public ChartConfiguration CreateSupercategoryAmountChart()
        {
            return CreateBarChart(
                SupercategoricalSpending.Select(q => q.SupercategoryName).ToList(),
                SupercategoricalSpending.Select(q => q.Amount).Cast<object>().ToList(),
                (TooltipFormatAmountFunction, TicksFormatAmountFunction));
        }

        public ChartConfiguration CreateSupercategorySpendingPercentChart()
        {
            var supercategories = new List<string>();
            var totalSupercategoricalSpending = 0m;
            var percentages = new List<double>();

            foreach (var (supercategory, amount) in SupercategoricalSpending)
            {
                if (amount < 0)
                    continue;
                totalSupercategoricalSpending += amount;
                supercategories.Add(supercategory);
                percentages.Add((double)(amount / TotalSpending));
            }

            if (totalSupercategoricalSpending < TotalSpending)
            {
                supercategories.Insert(0, MidasConstants.DefaultSupercategory);
                percentages.Insert(0, (double)((TotalSpending - totalSupercategoricalSpending) / TotalSpending));
            }

            return CreateDoughnutChart(supercategories, percentages.Cast<object>().ToList(), TooltipFormatPercentageFunction);
        }

        public ChartConfiguration CreateSupercategoryIncomePercentChart()
        {
            var supercategories = new List<string>();
            var totalSupercategoricalSpending = 0m;
            var percentages = new List<double>();

            foreach (var (supercategory, amount) in SupercategoricalSpending)
            {
                if (amount < 0)
                    continue;
                totalSupercategoricalSpending += amount;
                supercategories.Add(supercategory);
                percentages.Add((double)(amount / TotalIncome));
            }

            if (totalSupercategoricalSpending < TotalIncome)
            {
                supercategories.Add("Unspent");
                percentages.Add((double)((TotalIncome - totalSupercategoricalSpending) / TotalIncome));
            }

            return CreateDoughnutChart(supercategories, percentages.Cast<object>().ToList(), TooltipFormatPercentageFunction);
        }
    }
}
