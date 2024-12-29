using Haondt.Core.Extensions;
using SpendLess.Accounts.Services;
using SpendLess.Core.Extensions;
using SpendLess.Dashboard.Models;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Services;

namespace SpendLess.Dashboard.Services
{
    public class DashboardService(ITransactionService transactionService,
        IAccountsService accountsService) : IDashboardService
    {

        public async Task<DashboardDataDto> GatherData(DateTime start, DateTime end)
        {
            var transactions = await transactionService.GetTransactions(new()
            {
                TransactionFilter.MinDate(start),
                TransactionFilter.MaxDate(end)
            });

            var accounts = await accountsService.GetAccountsIncludedInNetWorth();
            var accountsList = accounts.Keys.ToList();
            var startAmounts = await transactionService.GetAmounts(new List<TransactionFilter>
            {
                TransactionFilter.EitherAccountIsOneOf(accountsList),
                TransactionFilter.ExclusiveMaxDate(start)
            });
            var currentBalances = accounts.ToDictionary(kvp => kvp.Key,
                kvp => startAmounts.ByDestination.GetValue(kvp.Key).Or(0) - startAmounts.BySource.GetValue(kvp.Key).Or(0));

            var transactionsByDateTime = transactions
                .Select(t =>
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds(t.Value.TimeStamp).DateTime;
                    var rounded = new DateTime(dt.Year, dt.Month, dt.Day);
                    return new { Date = rounded, Transaction = t.Value };
                })
                .GroupBy(t => t.Date)
                .ToDictionary(grp => grp.Key, grp => grp.Select(x => x.Transaction));

            var balancChartData = new DashboardBalanceChartDataDto
            {
                AccountNames = accountsList.Select(a => accounts[a].Name).ToList(),
                Balances = accountsList.Select(_ => new List<decimal>()).ToList(),
                TimeStamps = []
            };

            var currentDay = new DateTime(start.Year, start.Month, start.Day);
            if (start <= DateTime.MinValue.AddDays(7) || start <= DateTimeOffset.MinValue.DateTime.AddDays(7)) // subtracting a week as a buffer for localized time
                currentDay = transactionsByDateTime.Keys.Min();
            var lastDay = new DateTime(end.Year, end.Month, end.Day);
            if (end >= DateTime.MaxValue.AddDays(-7) || end >= DateTimeOffset.MaxValue.DateTime.AddDays(-7))
                lastDay = transactionsByDateTime.Keys.Max();

            while (currentDay <= lastDay)
            {
                if (transactionsByDateTime.TryGetValue(currentDay, out var currentTransactions))
                    foreach (var transaction in currentTransactions)
                    {
                        if (currentBalances.ContainsKey(transaction.SourceAccount))
                            currentBalances[transaction.SourceAccount] -= transaction.Amount;
                        if (currentBalances.ContainsKey(transaction.DestinationAccount))
                            currentBalances[transaction.DestinationAccount] += transaction.Amount;
                    }

                balancChartData.TimeStamps.Add(currentDay);
                for (int i = 0; i < accountsList.Count; i++)
                    balancChartData.Balances[i].Add(currentBalances[accountsList[i]]);

                currentDay = currentDay.AddDays(1);
            }

            var categoricalSpendingChartData = new Dictionary<string, decimal>();

            var flow = 0m;
            var income = 0m;
            var spending = 0m;
            foreach (var (_, transaction) in transactions)
            {
                if (accounts.ContainsKey(transaction.SourceAccount))
                    if (!accounts.ContainsKey(transaction.DestinationAccount)) // exclude transfers
                    {
                        flow -= transaction.Amount;
                        spending += transaction.Amount;
                        if (!categoricalSpendingChartData.TryGetValue(transaction.Category, out var amount))
                            amount = categoricalSpendingChartData[transaction.Category] = 0;
                        categoricalSpendingChartData[transaction.Category] = amount + transaction.Amount;
                    }

                if (accounts.ContainsKey(transaction.DestinationAccount)
                    && !accounts.ContainsKey(transaction.SourceAccount))
                {
                    flow += transaction.Amount;
                    income += transaction.Amount;
                }
            }

            var categorialSpendingChartKeys = categoricalSpendingChartData.Keys;

            return new()
            {
                CashFlow = flow,
                BalanceChartData = balancChartData,
                CategoricalSpendingChartData = new DashboardCategoricalSpendingChartDataDto
                {
                    Categories = categorialSpendingChartKeys.ToList(),
                    Spending = categorialSpendingChartKeys.Select(k => categoricalSpendingChartData[k]).ToList()
                },
                Income = income,
                Spending = spending,
                NetWorth = currentBalances.Values.Sum()
            };
        }
    }
}
