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
                TransactionFilter.Date.IsGreaterThanOrEqualTo(start),
                TransactionFilter.Date.IsLessThanOrEqualTo(end)
            });

            var accounts = await accountsService.GetAccountsIncludedInNetWorth();
            var accountsList = accounts.Keys.ToList();
            var startAmounts = await transactionService.GetAmounts(new List<TransactionFilter>
            {
                TransactionFilter.EitherAccountId.IsOneOf(accountsList),
                TransactionFilter.Date.IsLessThan(start)
            });
            var currentBalances = accounts.ToDictionary(kvp => kvp.Key,
                kvp => startAmounts.ByDestination.GetValue(kvp.Key).Or(0) - startAmounts.BySource.GetValue(kvp.Key).Or(0));

            List<(AbsoluteDateTime Date, IEnumerable<TransactionDto> Transactions)> transactionsByDateTime = transactions
                .Select(t =>
                {
                    return new { Date = t.Value.TimeStamp.FloorToLocalDay(), Transaction = t.Value };
                })
                .GroupBy(t => t.Date)
                .OrderBy(t => t.Key)
                .Select(grp =>
                (
                    grp.Key,
                    grp.Select(q => q.Transaction)
                ))
                .ToList();


            var startDay = start.FloorToLocalDay();
            if (start <= AbsoluteDateTime.MinValue.AddDays(7))// subtracting a week as a buffer for localized time
                startDay = transactionsByDateTime[0].Date;
            var endDay = end.FloorToLocalDay();
            if (end >= AbsoluteDateTime.MaxValue.AddDays(-7))
                endDay = transactionsByDateTime[^1].Date;

            var range = endDay - startDay;
            TimeStepper stepper;
            var desiredMaxPoints = 25;
            if (range <= TimeSpan.FromDays(desiredMaxPoints))
                stepper = new TimeStepper(startDay, TimeStepSize.Day);
            else if (range <= TimeSpan.FromDays(desiredMaxPoints * 7))
                stepper = new TimeStepper(startDay, TimeStepSize.Week);
            else if (range <= TimeSpan.FromDays(desiredMaxPoints * 31)) // 30 months
            {
                startDay = startDay.FloorToLocalMonth();
                stepper = new TimeStepper(startDay, TimeStepSize.Month);
            }
            else if (range <= TimeSpan.FromDays(desiredMaxPoints * 31 * 3)) // 30 quarters
            {
                startDay = startDay.FloorToLocalMonth();
                stepper = new TimeStepper(startDay, TimeStepSize.Quarter);
            }
            else
            {
                startDay = startDay.FloorToLocalYear();
                stepper = new TimeStepper(startDay, TimeStepSize.Year);
            }

            var transactionsByPeriod = new List<List<TransactionDto>>();
            var periods = new List<AbsoluteDateTime>();

            var currentPeriodStart = stepper.AbsoluteDateTime;
            periods.Add(currentPeriodStart);
            transactionsByPeriod.Add(new());
            stepper = stepper.Step();
            var currentPeriodEnd = stepper.AbsoluteDateTime;
            foreach (var (date, transactionGroup) in transactionsByDateTime)
            {
                while (date >= currentPeriodEnd)
                {
                    currentPeriodStart = currentPeriodEnd;
                    periods.Add(currentPeriodStart);
                    transactionsByPeriod.Add(new());
                    stepper = stepper.Step();
                    currentPeriodEnd = stepper.AbsoluteDateTime;
                }

                transactionsByPeriod[^1].AddRange(transactionGroup);
            }

            while (currentPeriodEnd <= endDay)
            {
                currentPeriodStart = currentPeriodEnd;
                periods.Add(currentPeriodStart);
                transactionsByPeriod.Add(new());
                stepper = stepper.Step();
                currentPeriodEnd = stepper.AbsoluteDateTime;
            }

            var balanceChartData = new DashboardBalanceChartDataDto
            {
                AccountNames = accountsList.Select(a => accounts[a].Name).Prepend("Net Worth").ToList(),
                Balances = Enumerable.Range(0, accountsList.Count + 1).Select(_ => new List<decimal>(new decimal[periods.Count])).ToList(),
                TimeStepLabels = stepper.StepSize switch
                {
                    TimeStepSize.Day => periods.Select(p => p.LocalTime.ToString("yyyy-MM-dd")).ToList(),
                    TimeStepSize.Week => periods.Select(p => p.LocalTime.ToString("yyyy-MM-dd")).ToList(),
                    TimeStepSize.Month => periods.Select(p => p.LocalTime.ToString("MMMM yyyy")).ToList(),
                    TimeStepSize.Quarter => periods.Select(p => p.LocalTime.ToString("MMMM yyyy")).ToList(),
                    TimeStepSize.Year => periods.Select(p => p.LocalTime.ToString("yyyy")).ToList(),
                    _ => throw new ArgumentException($"Unknown step size {stepper.StepSize}")
                }
            };

            var accountIndices = accountsList.Select((id, index) => (id, index))
                .ToDictionary(q => q.id, q => q.index + 1);
            for (int i = 0; i < transactionsByPeriod.Count; i++)
            {
                foreach (var transaction in transactionsByPeriod[i])
                {
                    if (accountIndices.TryGetValue(transaction.SourceAccount, out var index))
                    {
                        balanceChartData.Balances[0][i] += transaction.Amount;
                        balanceChartData.Balances[index][i] += transaction.Amount;
                    }
                    if (accountIndices.TryGetValue(transaction.DestinationAccount, out index))
                    {
                        balanceChartData.Balances[0][i] -= transaction.Amount;
                        balanceChartData.Balances[index][i] -= transaction.Amount;
                    }
                }
            }


            //while (currentDay <= lastDay)
            //{
            //    if (transactionsByDateTime.TryGetValue(currentDay, out var currentTransactions))
            //        foreach (var transaction in currentTransactions)
            //        {
            //            if (currentBalances.ContainsKey(transaction.SourceAccount))
            //                currentBalances[transaction.SourceAccount] -= transaction.Amount;
            //            if (currentBalances.ContainsKey(transaction.DestinationAccount))
            //                currentBalances[transaction.DestinationAccount] += transaction.Amount;
            //        }

            //    balanceChartData.TimeStamps.Add(currentDay);
            //    balanceChartData.Balances[0].Add(0);
            //    for (int i = 0; i < accountsList.Count; i++)
            //    {
            //        balanceChartData.Balances[i + 1].Add(currentBalances[accountsList[i]]);
            //        balanceChartData.Balances[0][^1] += currentBalances[accountsList[i]];
            //    }
            //    currentDay = currentDay.AddLocalDays(1);
            //}

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
                TransactionFilter.EitherAccountId.IsOneOf(accountsList),
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
