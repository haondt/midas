using SpendLess.Domain.Accounts.Services;
using SpendLess.Persistence.Models;
using SpendLess.UI.Models.Transactions;

namespace SpendLess.UI.Services.Transactions
{
    public class TransactionFilterService(IAccountsService accountsService) : ITransactionFilterService
    {
        public async Task<IEnumerable<TransactionFilter>> ParseFiltersAsync(IEnumerable<string> filters)
        {
            var tasks = filters.Select(async filter =>
            {
                var splitFilter = filter.Split(' ');
                var target = splitFilter[0];
                var op = splitFilter[1];
                var value = string.Join(' ', splitFilter[2..]);

                switch (target)
                {
                    case TransactionFilterTargets.Description:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.DescriptionContains(value);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Category:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.HasCategory(value);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Amount:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsGreaterThanOrEqualTo:
                                return TransactionFilter.MinAmount(decimal.Parse(value));
                            case TransactionFilterOperators.IsLessThanOrEqualTo:
                                return TransactionFilter.MaxAmount(decimal.Parse(value));
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.HasAmount(decimal.Parse(value));
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Tags:
                        switch (op)
                        {
                            case TransactionFilterOperators.Contains:
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.HasTag(value);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.SourceAccountName:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                var accountsList = await accountsService.GetAccountIdsByName(value);
                                return TransactionFilter.SourceAccountIsOneOf(accountsList);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.DestinationAccountName:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                var accountsList = await accountsService.GetAccountIdsByName(value);
                                return TransactionFilter.DestinationAccountIsOneOf(accountsList);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.EitherAccountName:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                var accountsList = await accountsService.GetAccountIdsByName(value);
                                return TransactionFilter.EitherAccountIsOneOf(accountsList);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    default:
                        throw new NotSupportedException($"Target {target} is not supported.");
                }
            });

            return await Task.WhenAll(tasks);
        }
    }
}
