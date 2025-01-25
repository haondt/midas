using Midas.Core.Models;
using Midas.Domain.Accounts.Services;
using Midas.Persistence.Models;
using Midas.UI.Models.Transactions;

namespace Midas.UI.Services.Transactions
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
                            case TransactionFilterOperators.IsNotEqualTo:
                                return TransactionFilter.CategoryIsNot(value);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Supercategory:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.SupercategoryIs(value);
                            case TransactionFilterOperators.IsNoneOrEqualTo:
                                return TransactionFilter.SupercategoryIsNoneOrEqualTo(value);
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
                    case TransactionFilterTargets.Id:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsOneOf:
                                return TransactionFilter.TransactionIdIsOneOf(value
                                    .Split(',')
                                    .Select(q => (long.TryParse(q, out var id), id))
                                    .Where(q => q.Item1)
                                    .Select(q => q.id)
                                    .ToList());
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.SourceAccountId:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.SourceAccountIsOneOf([value]);
                            case TransactionFilterOperators.IsNotEqualTo:
                                return TransactionFilter.SourceAccountIsNotOneOf([value]);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.DestinationAccountId:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                return TransactionFilter.DestinationAccountIsOneOf([value]);
                            case TransactionFilterOperators.IsNotEqualTo:
                                return TransactionFilter.DestinationAccountIsNotOneOf([value]);
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.SourceAccountName:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                {
                                    var accountsList = await accountsService.GetAccountIdsByName(value);
                                    return TransactionFilter.SourceAccountIsOneOf(accountsList);
                                }
                            case TransactionFilterOperators.IsNotEqualTo:
                                {
                                    var accountsList = await accountsService.GetAccountIdsByName(value);
                                    return TransactionFilter.SourceAccountIsNotOneOf(accountsList);
                                }
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.DestinationAccountName:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                {
                                    var accountsList = await accountsService.GetAccountIdsByName(value);
                                    return TransactionFilter.DestinationAccountIsOneOf(accountsList);
                                }
                            case TransactionFilterOperators.IsNotEqualTo:
                                {
                                    var accountsList = await accountsService.GetAccountIdsByName(value);
                                    return TransactionFilter.DestinationAccountIsNotOneOf(accountsList);
                                }
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.EitherAccountName:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                {
                                    var accountsList = await accountsService.GetAccountIdsByName(value);
                                    return TransactionFilter.EitherAccountIsOneOf(accountsList);
                                }
                            default:
                                throw new NotSupportedException($"Operation {op} is not supported with target {target}.");
                        }
                    case TransactionFilterTargets.Date:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsGreaterThanOrEqualTo:
                                {
                                    var dateTime = StringFormatter.ParseDate(value);
                                    return TransactionFilter.MinDate(dateTime);
                                }
                            case TransactionFilterOperators.IsLessThanOrEqualTo:
                                {
                                    var dateTime = StringFormatter.ParseDate(value);
                                    dateTime = dateTime.AddLocalDays(1).FloorToLocalDay();
                                    return TransactionFilter.ExclusiveMaxDate(dateTime);
                                }
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
