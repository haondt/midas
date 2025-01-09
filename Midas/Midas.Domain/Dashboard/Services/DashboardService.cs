using Haondt.Core.Extensions;
using Midas.Core.Extensions;
using Midas.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Dashboard.Models;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;

namespace Midas.Domain.Dashboard.Services
{
    public class DashboardService(ITransactionService transactionService,
        IAccountsService accountsService) : IDashboardService
    {

        public async Task<DashboardDataDto> GatherData(AbsoluteDateTime start, AbsoluteDateTime end)
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
                    return new { Date = t.Value.TimeStamp.FloorToLocalDay(), Transaction = t.Value };
                })
                .GroupBy(t => t.Date)
                .ToDictionary(grp => grp.Key, grp => grp.Select(x => x.Transaction));

            var balanceChartData = new DashboardBalanceChartDataDto
            {
                AccountNames = accountsList.Select(a => accounts[a].Name).Prepend("Net Worth").ToList(),
                Balances = accountsList.Select(_ => new List<decimal>()).Prepend(new List<decimal>()).ToList(),
                TimeStamps = []
            };

            var currentDay = start.FloorToLocalDay();
            if (start <= AbsoluteDateTime.MinValue.AddDays(7)) // basically, if we are using the min & max datetimes (with some tolerance), just round to the earliest and latest transactions
                currentDay = transactionsByDateTime.Keys.Min();
            var lastDay = end.FloorToLocalDay();
            if (end >= AbsoluteDateTime.MaxValue.AddDays(-7))
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

                balanceChartData.TimeStamps.Add(currentDay);
                balanceChartData.Balances[0].Add(0);
                for (int i = 0; i < accountsList.Count; i++)
                {
                    balanceChartData.Balances[i + 1].Add(currentBalances[accountsList[i]]);
                    balanceChartData.Balances[0][^1] += currentBalances[accountsList[i]];
                }
                currentDay = currentDay.AddLocalDays(1);
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

            var netAmounts = await transactionService.GetAmounts(new List<TransactionFilter>
            {
                TransactionFilter.EitherAccountIsOneOf(accountsList),
            });
            var netWorth = 0m;
            foreach (var (k, v) in netAmounts.BySource)
                if (accountsList.Contains(k))
                    netWorth -= v;
            foreach (var (k, v) in netAmounts.ByDestination)
                if (accountsList.Contains(k))
                    netWorth += v;

            return new()
            {
                CashFlow = flow,
                BalanceChartData = balanceChartData,
                CategoricalSpendingChartData = new DashboardCategoricalSpendingChartDataDto
                {
                    Categories = categorialSpendingChartKeys.ToList(),
                    Spending = categorialSpendingChartKeys.Select(k => categoricalSpendingChartData[k]).ToList()
                },
                Income = income,
                Spending = spending,
                NetWorth = netWorth
            };
        }
    }
}
