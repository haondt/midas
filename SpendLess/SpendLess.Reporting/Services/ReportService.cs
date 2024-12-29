using SpendLess.Accounts.Services;
using SpendLess.Domain.Models;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Services;

namespace SpendLess.Reporting.Services
{
    public class ReportService(ITransactionService transactionService,
        IAccountsService accountsService) : IReportService
    {
        public async Task<ReportDataDto> GenerateReportData(DateTime start, DateTime end)
        {
            var transactions = await transactionService.GetTransactions(new()
            {
                TransactionFilter.MinDate(start),
                TransactionFilter.MaxDate(end)
            });

            List<(DateTime Date, IEnumerable<TransactionDto> Transactions)> transactionsByDateTime = transactions
                .Select(t =>
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds(t.Value.TimeStamp).DateTime;
                    var rounded = new DateTime(dt.Year, dt.Month, dt.Day);
                    return new { Date = rounded, Transaction = t.Value };
                })
                .GroupBy(t => t.Date)
                .OrderBy(t => t.Key)
                .Select(grp =>
                (
                    grp.Key,
                    grp.Select(q => (q.Transaction))
                ))
                .ToList();

            var startDay = new DateTime(start.Year, start.Month, start.Day);
            if (start <= DateTime.MinValue.AddDays(7) || start <= DateTimeOffset.MinValue.DateTime.AddDays(7)) // subtracting a week as a buffer for localized time
                startDay = transactionsByDateTime[0].Date;
            var endDay = new DateTime(end.Year, end.Month, end.Day);
            if (end >= DateTime.MaxValue.AddDays(-7) || end >= DateTimeOffset.MaxValue.DateTime.AddDays(-7))
                endDay = transactionsByDateTime[^1].Date;

            var range = endDay - startDay;
            TimeStepper stepper;
            if (range <= TimeSpan.FromDays(95)) // ~ 3 months
                stepper = new TimeStepper(startDay, TimeStepSize.Day);
            else if (range <= TimeSpan.FromDays(550)) // ~ 1.5 years
                stepper = new TimeStepper(startDay, TimeStepSize.Month);
            else
                stepper = new TimeStepper(startDay, TimeStepSize.Year);

            var transactionsByPeriod = new List<List<TransactionDto>>();
            var periods = new List<DateTime>();

            var currentPeriodStart = stepper.DateTime;
            periods.Add(currentPeriodStart);
            transactionsByPeriod.Add(new());
            stepper = stepper.Step();
            var currentPeriodEnd = stepper.DateTime;

            foreach (var (date, transactionGroup) in transactionsByDateTime)
            {
                if (date >= currentPeriodEnd)
                {
                    currentPeriodStart = currentPeriodEnd;
                    periods.Add(currentPeriodStart);
                    transactionsByPeriod.Add(new());
                    stepper = stepper.Step();
                    currentPeriodEnd = stepper.DateTime;
                }

                transactionsByPeriod[^1].AddRange(transactionGroup);
            }

            return new();
        }
    }
}
