using Midas.Core.Models;

namespace Midas.Domain.Reporting.Models
{
    public class ReportDataDto
    {
        public required AbsoluteDateTime InclusiveStartDay { get; set; }
        public required AbsoluteDateTime InclusiveEndDay { get; set; }

        public required List<string> TimeStepLabels { get; set; }
        public required List<decimal> SpendingPerTimeStep { get; set; }
        public required List<decimal> IncomePerTimeStep { get; set; }
        public required List<decimal> CashFlowPerTimeStep { get; set; }
        public required List<(string CategoryName, List<decimal> Amounts)> CategoricalSpendingPerTimeStep { get; set; }

        public required List<(string AccountName, decimal Income, decimal Spending, decimal CashFlow)> AccountSpending { get; set; }
        public required (decimal Income, decimal Spending, decimal CashFlow) TotalSpending { get; set; }
        public required List<(string CategoryName, decimal Amount)> CategoricalSpending { get; set; }
        public required List<(string SupercategoryName, decimal Amount)> SupercategoricalSpending { get; set; }

        public required List<(string AccountName, decimal Amount, int TransactionsCount, decimal AverageAmountPerTransaction)> TopIncomeSources { get; set; }
        public required List<(string AccountName, decimal Amount, int TransactionsCount, decimal AverageAmountPerTransaction)> TopSpendingDestinations { get; set; }
    }
}