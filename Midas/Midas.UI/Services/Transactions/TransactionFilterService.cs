using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Midas.Core.Extensions;
using Midas.Core.Models;
using Midas.Persistence.Models;
using Midas.UI.Models.Transactions;

namespace Midas.UI.Services.Transactions
{
    public class TransactionFilterService : ITransactionFilterService
    {

        private Optional<TransactionFilter> TryGetFilter<T>(
            string op, string value,
            TransactionFilterBuilder<T> builder,
            Func<string, Optional<T>> parseValue, Func<string, Optional<List<T>>> parseValues) where T : notnull
        {
            Optional<Func<TransactionFilterBuilder<T>, Optional<TransactionFilter>>> converter = op switch
            {
                TransactionFilterOperators.IsEqualTo => new(x => parseValue(value).As(y => x.Is(y))),
                TransactionFilterOperators.IsOneOf => new(x => parseValues(value).As(y => x.IsOneOf(y))),
                TransactionFilterOperators.IsNotEqualTo => new(x => parseValue(value).As(y => x.IsNot(y))),
                TransactionFilterOperators.Contains => new(x => x switch
                    {
                        StringTransactionFilterBuilder sx => parseValue(value).TryCast<T, string>().As(y => sx.Contains((string)y)),
                        _ => new()
                    }),
                TransactionFilterOperators.StartsWith => new(x => x switch
                    {
                        StringTransactionFilterBuilder sx => parseValue(value).TryCast<T, string>().As(y => sx.StartsWith((string)y)),
                        _ => new()
                    }),
                TransactionFilterOperators.EndsWith => new(x => x switch
                    {
                        StringTransactionFilterBuilder sx => parseValue(value).TryCast<T, string>().As(y => sx.EndsWith((string)y)),
                        _ => new()
                    }),
                TransactionFilterOperators.IsGreaterThanOrEqualTo => new(x => parseValue(value).As(y => x.IsGreaterThanOrEqualTo(y))),
                TransactionFilterOperators.IsLessThanOrEqualTo => new(x => parseValue(value).As(y => x.IsLessThanOrEqualTo(y))),
                TransactionFilterOperators.IsGreaterThan => new(x => parseValue(value).As(y => x.IsGreaterThan(y))),
                TransactionFilterOperators.IsLessThan => new(x => parseValue(value).As(y => x.IsLessThan(y))),
                TransactionFilterOperators.IsNoneOrEqualTo => new(x => parseValue(value).As(y => x.IsNoneOr(y))),
                TransactionFilterOperators.IsNone => new(static x => x.IsNone()),
                TransactionFilterOperators.IsNotNone => new(static x => x.IsNotNone()),
                _ => new()
            };

            return converter.As(q => q(builder)).Or(new Optional<TransactionFilter>());
        }


        // todo: un-taskify this
        public Task<IEnumerable<TransactionFilter>> ParseFiltersAsync(IEnumerable<string> filters)
        {
            var result = filters.Select(filter =>
            {
                var splitFilter = filter.Split(' ');
                var target = splitFilter[0];
                var op = splitFilter[1];
                var value = string.Join(' ', splitFilter[2..]);

                var transactionFilter = target switch
                {
                    TransactionFilterTargets.Amount => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Amount,
                        static x => decimal.Parse(x),
                        static x => throw new NotSupportedException($"Cannot convert number to a list of numbers")),
                    TransactionFilterTargets.Description => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Description,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split description")),
                    TransactionFilterTargets.Category => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Category,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split category")),
                    TransactionFilterTargets.Supercategory => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Supercategory,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split supercategory")),
                    TransactionFilterTargets.SourceAccountName => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.SourceAccountName,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split account name")),
                    TransactionFilterTargets.SourceAccountId => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.SourceAccountId,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split account id")),
                    TransactionFilterTargets.DestinationAccountName => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.DestinationAccountName,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split account name")),
                    TransactionFilterTargets.DestinationAccountId => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.DestinationAccountId,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split account id")),
                    TransactionFilterTargets.EitherAccountName => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.EitherAccountName,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split account name")),
                    TransactionFilterTargets.EitherAccountId => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.EitherAccountId,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split account id")),
                    TransactionFilterTargets.Date => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Date,
                        static x => StringFormatter.ParseDate(x),
                        static x => x.Split(',')
                            .Select(q => StringFormatter.ParseDate(q))
                            .ToList()),
                    TransactionFilterTargets.Tags => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Tags,
                        static x => x.Trim(),
                        static x => throw new NotSupportedException($"Cannot split tags")),
                    TransactionFilterTargets.Id => TryGetFilter(
                        op,
                        value,
                        TransactionFilter.Id,
                        static x => long.Parse(x),
                        static x => x.Split(',')
                            .Select(q => (long.TryParse(q, out var id), id))
                            .Where(q => q.Item1)
                            .Select(q => q.id)
                            .ToList()),
                    _ => new()
                };

                return transactionFilter.Or(() => throw new NotSupportedException($"Operation {op} is not supported with target {target}."));
            });

            return Task.FromResult(result);
        }
    }
}
