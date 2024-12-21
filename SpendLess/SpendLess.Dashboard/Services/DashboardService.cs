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

            var ownedAccounts = await accountsService.GetOwned();

            var flow = 0m;
            foreach (var (_, transaction) in transactions)
            {
                if (ownedAccounts.ContainsKey(transaction.SourceAccount)
                    && !ownedAccounts.ContainsKey(transaction.DestinationAccount)) // exclude transfers
                    flow -= transaction.Amount;
                if (ownedAccounts.ContainsKey(transaction.DestinationAccount)
                    && !ownedAccounts.ContainsKey(transaction.SourceAccount))
                    flow += transaction.Amount;
            }

            return new()
            {
                CashFlow = flow
            };
        }
    }
}
