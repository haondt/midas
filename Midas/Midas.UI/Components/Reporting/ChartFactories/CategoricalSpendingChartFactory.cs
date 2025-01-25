using Midas.Core.Constants;
using Midas.UI.Models.Charts;

namespace Midas.UI.Components.Reporting.ChartFactories
{
    public partial class ReportingChartFactory
    {
        public required List<(string CategoryName, decimal Amount)> CategoricalSpending { get; set; }

        public ChartConfiguration CreateCategoryAmountChart()
        {
            return CreateBarChart(
                CategoricalSpending.Select(q => q.CategoryName).ToList(),
                CategoricalSpending.Select(q => q.Amount).Cast<object>().ToList(),
                (TooltipFormatAmountFunction, TicksFormatAmountFunction));
        }

        public ChartConfiguration CreateCategorySpendingPercentChart()
        {
            var categories = new List<string>();
            var totalCategoricalSpending = 0m;
            var percentages = new List<double>();

            foreach (var (category, amount) in CategoricalSpending)
            {
                if (amount < 0)
                    continue;
                totalCategoricalSpending += amount;
                categories.Add(category);
                percentages.Add((double)(amount / TotalSpending));
            }

            if (totalCategoricalSpending < TotalSpending)
            {
                categories.Insert(0, MidasConstants.DefaultCategory);
                percentages.Insert(0, (double)((TotalSpending - totalCategoricalSpending) / TotalSpending));
            }

            return CreateDoughnutChart(categories, percentages.Cast<object>().ToList(), TooltipFormatPercentageFunction);
        }

        public ChartConfiguration CreateCategoryIncomePercentChart()
        {
            var categories = new List<string>();
            var totalCategoricalSpending = 0m;
            var percentages = new List<double>();

            foreach (var (category, amount) in CategoricalSpending)
            {
                if (amount < 0)
                    continue;
                totalCategoricalSpending += amount;
                categories.Add(category);
                percentages.Add((double)(amount / TotalIncome));
            }

            if (totalCategoricalSpending < TotalIncome)
            {
                categories.Add("Unspent");
                percentages.Add((double)((TotalIncome - totalCategoricalSpending) / TotalIncome));
            }

            return CreateDoughnutChart(categories, percentages.Cast<object>().ToList(), TooltipFormatPercentageFunction);
        }
    }
}
