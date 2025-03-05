using Haondt.Core.Models;
using Microsoft.Extensions.Options;
using Midas.Core.Constants;
using Midas.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Domain.Reporting.Models;
using Midas.Domain.Supercategories.Services;
using Midas.Domain.Transactions.Services;
using Midas.Persistence.Models;

namespace Midas.Domain.Reporting.Services
{
    public class ReportService(ITransactionService transactionService,
        IAccountsService accountsService,
        ISupercategoryService supercategoryService,
        IOptions<ReportingSettings> options) : IReportService
    {
        public async Task<ReportDataDto> GenerateReportData(AbsoluteDateTime start, AbsoluteDateTime end)
        {
            var inclusiveStartTime = start.FloorToLocalDay();
            var exclusiveEndTime = end.FloorToLocalDay().AddDays(1);
            var transactions = await transactionService.GetTransactions(new()
            {
                TransactionFilter.Date.IsGreaterThanOrEqualTo(inclusiveStartTime),
                TransactionFilter.Date.IsLessThan(exclusiveEndTime)
            });

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

            if (start <= AbsoluteDateTime.MinValue.AddDays(7))// subtracting a week as a buffer for localized time
                inclusiveStartTime = transactionsByDateTime[0].Date;
            if (end >= AbsoluteDateTime.MaxValue.AddDays(-7))
                exclusiveEndTime = transactionsByDateTime[^1].Date.AddDays(1);

            var range = exclusiveEndTime - inclusiveStartTime;
            TimeStepper stepper;
            if (range <= TimeSpan.FromDays(95)) // ~ 3 months
                stepper = new TimeStepper(inclusiveStartTime, TimeStepSize.Day);
            else if (range <= TimeSpan.FromDays(550)) // ~ 1.5 years
            {
                inclusiveStartTime = inclusiveStartTime.FloorToLocalMonth();
                stepper = new TimeStepper(inclusiveStartTime, TimeStepSize.Month);
            }
            else
            {
                inclusiveStartTime = inclusiveStartTime.FloorToLocalYear();
                stepper = new TimeStepper(inclusiveStartTime, TimeStepSize.Year);
            }

            var transactionsByPeriod = new List<List<TransactionDto>>();
            var periods = new List<AbsoluteDateTime>();

            var currentPeriodInclusiveStart = stepper.AbsoluteDateTime;
            periods.Add(currentPeriodInclusiveStart);
            transactionsByPeriod.Add(new());
            stepper = stepper.Step();
            var currentPeriodExclusiveEnd = stepper.AbsoluteDateTime;

            foreach (var (date, transactionGroup) in transactionsByDateTime)
            {
                while (date >= currentPeriodExclusiveEnd)
                {
                    currentPeriodInclusiveStart = currentPeriodExclusiveEnd;
                    periods.Add(currentPeriodInclusiveStart);
                    transactionsByPeriod.Add(new());
                    stepper = stepper.Step();
                    currentPeriodExclusiveEnd = stepper.AbsoluteDateTime;
                }

                transactionsByPeriod[^1].AddRange(transactionGroup);
            }

            while (currentPeriodExclusiveEnd < exclusiveEndTime)
            {
                currentPeriodInclusiveStart = currentPeriodExclusiveEnd;
                periods.Add(currentPeriodInclusiveStart);
                transactionsByPeriod.Add(new());
                stepper = stepper.Step();
                currentPeriodExclusiveEnd = stepper.AbsoluteDateTime;
            }

            var reportData = new ReportDataDto
            {
                InclusiveStartDay = inclusiveStartTime,
                InclusiveEndDay = exclusiveEndTime.AddLocalDays(-1),

                TimeStepLabels = stepper.StepSize switch
                {
                    TimeStepSize.Day => periods.Select(p => p.LocalTime.ToString("yyyy-MM-dd")).ToList(),
                    TimeStepSize.Month => periods.Select(p => p.LocalTime.ToString("MMMM yyyy")).ToList(),
                    TimeStepSize.Year => periods.Select(p => p.LocalTime.ToString("yyyy")).ToList(),
                    _ => throw new ArgumentException($"Unknown step size {stepper.StepSize}")
                },
                SpendingPerTimeStep = [],
                IncomePerTimeStep = [],
                CashFlowPerTimeStep = [],
                CategoricalSpendingPerTimeStep = [],
                AccountSpending = [],
                TotalSpending = (0, 0, 0),
                CategoricalSpending = [],
                SupercategoricalSpending = [],
                TopIncomeSources = [],
                TopSpendingDestinations = [],
            };

            var myAccounts = await accountsService.GetAccountsIncludedInNetWorth();
            var categories = await transactionService.GetCategories();
            var categoricalSpending = categories.ToDictionary(c => c, _ => 0m);
            var categoricalSpendingPerTimeStep = categories.ToDictionary(c => c, _ => new List<decimal>());
            var amountPerIncomeSource = new Dictionary<string, (string Name, decimal Amount, int Count)>();
            var amountPerSpendingDestination = new Dictionary<string, (string Name, decimal Amount, int Count)>();
            var accountAmounts = myAccounts
                .ToDictionary(a => a.Key, a => new
                {
                    a.Value.Name,
                    Income = 0m,
                    Spending = 0m,
                    CashFlow = 0m
                });

            foreach (var ts in transactionsByPeriod)
            {
                reportData.SpendingPerTimeStep.Add(0);
                reportData.IncomePerTimeStep.Add(0);
                reportData.CashFlowPerTimeStep.Add(0);
                foreach (var category in categories)
                    categoricalSpendingPerTimeStep[category].Add(0);
                foreach (var t in ts)
                {
                    if (myAccounts.ContainsKey(t.SourceAccount) && !myAccounts.ContainsKey(t.DestinationAccount))
                    {
                        reportData.SpendingPerTimeStep[^1] += t.Amount;
                        reportData.CashFlowPerTimeStep[^1] -= t.Amount;
                        reportData.TotalSpending = (reportData.TotalSpending.Income, reportData.TotalSpending.Spending + t.Amount, reportData.TotalSpending.CashFlow - t.Amount);
                        categoricalSpendingPerTimeStep[t.Category][^1] += t.Amount;
                        categoricalSpending[t.Category] += t.Amount;

                        if (!amountPerSpendingDestination.TryGetValue(t.DestinationAccount, out var destination))
                        {
                            var accountName = (await accountsService.GetAccount(t.DestinationAccount)).Name;
                            destination = amountPerSpendingDestination[t.DestinationAccount] = (accountName, 0m, 0);
                        }
                        amountPerSpendingDestination[t.DestinationAccount] = (destination.Name, destination.Amount + t.Amount, destination.Count + 1);

                        var accountAmount = accountAmounts[t.SourceAccount];
                        accountAmounts[t.SourceAccount] = accountAmounts[t.SourceAccount] with { Spending = accountAmount.Spending + t.Amount, CashFlow = accountAmount.CashFlow - t.Amount };
                    }
                    if (myAccounts.ContainsKey(t.DestinationAccount) && !myAccounts.ContainsKey(t.SourceAccount))
                    {
                        reportData.IncomePerTimeStep[^1] += t.Amount;
                        reportData.CashFlowPerTimeStep[^1] += t.Amount;
                        reportData.TotalSpending = (reportData.TotalSpending.Income + t.Amount, reportData.TotalSpending.Spending, reportData.TotalSpending.CashFlow + t.Amount);

                        if (!amountPerIncomeSource.TryGetValue(t.SourceAccount, out var source))
                        {
                            var accountName = (await accountsService.GetAccount(t.SourceAccount)).Name;
                            source = amountPerIncomeSource[t.SourceAccount] = (accountName, 0m, 0);
                        }
                        amountPerIncomeSource[t.SourceAccount] = (source.Name, source.Amount + t.Amount, source.Count + 1);

                        var accountAmount = accountAmounts[t.DestinationAccount];
                        accountAmounts[t.DestinationAccount] = accountAmounts[t.DestinationAccount] with { Income = accountAmount.Income + t.Amount, CashFlow = accountAmount.CashFlow + t.Amount };
                    }
                }
            }

            reportData.CategoricalSpendingPerTimeStep = categories
                .Where(c => categoricalSpending[c] > 0)
                .Select(c => (c, categoricalSpendingPerTimeStep[c]))
                .ToList();

            reportData.AccountSpending = accountAmounts
                .Select(kvp => (
                    kvp.Value.Name,
                    kvp.Value.Income,
                    kvp.Value.Spending,
                    kvp.Value.CashFlow
                ))
                .ToList();

            reportData.CategoricalSpending = categoricalSpending
                //.Where(c => categoricalSpending[c] > 0)
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => (kvp.Key, kvp.Value))
                .OrderByDescending(grp => grp.Value)
                .ToList();

            var supercategoryMap = new Dictionary<string, string>();
            foreach (var (supercategory, supercategoryCategories) in await supercategoryService.GetSupercategories())
                foreach (var category in supercategoryCategories)
                    supercategoryMap[category] = supercategory;
            reportData.SupercategoricalSpending = categoricalSpending
                .GroupBy(kvp => supercategoryMap.GetValueOrDefault(kvp.Key, MidasConstants.DefaultSupercategory))
                .Select(grp => (grp.Key, grp.Select(kvp => kvp.Value).Sum()))
                .Where(q => q.Item2 > 0)
                .OrderByDescending(q => q.Item2)
                .ToList();

            reportData.TopIncomeSources = amountPerIncomeSource
                .OrderByDescending(kvp => kvp.Value.Amount)
                .Take(options.Value.TopIncomeSourcesLimit)
                .Select(kvp => (kvp.Value.Name, kvp.Value.Amount, kvp.Value.Count, kvp.Value.Amount / kvp.Value.Count))
                .ToList();

            reportData.TopSpendingDestinations = amountPerSpendingDestination
                .OrderByDescending(kvp => kvp.Value.Amount)
                .Take(options.Value.TopSpendingDestinationsLimit)
                .Select(kvp => (kvp.Value.Name, kvp.Value.Amount, kvp.Value.Count, kvp.Value.Amount / kvp.Value.Count))
                .ToList();

            return reportData;
        }
    }
}
