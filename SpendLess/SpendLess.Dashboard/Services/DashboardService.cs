using SpendLess.Accounts.Services;
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

            var flow = 0m;
            foreach (var (_, transaction) in transactions)
            {
                if (accounts.ContainsKey(transaction.SourceAccount)
                    && !accounts.ContainsKey(transaction.DestinationAccount)) // exclude transfers
                    flow -= transaction.Amount;
                if (accounts.ContainsKey(transaction.DestinationAccount)
                    && !accounts.ContainsKey(transaction.SourceAccount))
                    flow += transaction.Amount;
            }

            return new()
            {
                CashFlow = flow
            };
        }
    }
}
