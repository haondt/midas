using Haondt.Core.Models;

namespace Midas.Persistence.Models
{

    public abstract class TransactionFilter
    {
        public static TransactionFilterBuilder<decimal> Amount { get; } = new TransactionFilterBuilder<decimal>(op => new AmountFilter { Operation = op });
        public static StringTransactionFilterBuilder Description { get; } = new StringTransactionFilterBuilder(op => new DescriptionFilter { Operation = op });
        public static StringTransactionFilterBuilder Category { get; } = new StringTransactionFilterBuilder(op => new CategoryFilter { Operation = op });
        public static StringTransactionFilterBuilder Supercategory { get; } = new StringTransactionFilterBuilder(op => new SupercategoryFilter { Operation = op });
        public static StringTransactionFilterBuilder SourceAccountName { get; } = new StringTransactionFilterBuilder(op => new SourceAccountNameFilter { Operation = op });
        public static TransactionFilterBuilder<string> SourceAccountId { get; } = new TransactionFilterBuilder<string>(op => new SourceAccountIdFilter { Operation = op });
        public static StringTransactionFilterBuilder DestinationAccountName { get; } = new StringTransactionFilterBuilder(op => new DestinationAccountNameFilter { Operation = op });
        public static TransactionFilterBuilder<string> DestinationAccountId { get; } = new TransactionFilterBuilder<string>(op => new DestinationAccountIdFilter { Operation = op });
        public static StringTransactionFilterBuilder EitherAccountName { get; } = new StringTransactionFilterBuilder(op => new EitherAccountNameFilter { Operation = op });
        public static TransactionFilterBuilder<string> EitherAccountId { get; } = new TransactionFilterBuilder<string>(op => new EitherAccountIdFilter { Operation = op });
        public static TransactionFilterBuilder<AbsoluteDateTime> Date { get; } = new TransactionFilterBuilder<AbsoluteDateTime>(op => new DateFilter { Operation = op });
        public static StringTransactionFilterBuilder Tags { get; } = new StringTransactionFilterBuilder(op => new TagsFilter { Operation = op });
        public static TransactionFilterBuilder<long> Id { get; } = new TransactionFilterBuilder<long>(op => new IdFilter { Operation = op });
        public static TransactionFilterBuilder<long> ImportSourceDataHash { get; } = new TransactionFilterBuilder<long>(op => new ImportSourceDataHashFilter { Operation = op });
    }

    public abstract class TransactionFilter<T> : TransactionFilter
    {
        public required TransactionFilterOperation<T> Operation { get; set; }
    }
    public class AmountFilter : TransactionFilter<decimal> { }
    public class CategoryFilter : TransactionFilter<string> { }
    public class DescriptionFilter : TransactionFilter<string> { }
    public class SupercategoryFilter : TransactionFilter<string> { }
    public class SourceAccountNameFilter : TransactionFilter<string> { }
    public class SourceAccountIdFilter : TransactionFilter<string> { }
    public class DestinationAccountNameFilter : TransactionFilter<string> { }
    public class DestinationAccountIdFilter : TransactionFilter<string> { }
    public class EitherAccountNameFilter : TransactionFilter<string> { }
    public class EitherAccountIdFilter : TransactionFilter<string> { }
    public class DateFilter : TransactionFilter<AbsoluteDateTime> { }
    public class ImportSourceDataHashFilter : TransactionFilter<long> { }
    public class TagsFilter : TransactionFilter<string> { }
    public class IdFilter : TransactionFilter<long> { }

    public class StringTransactionFilterBuilder(Func<TransactionFilterOperation<string>, TransactionFilter<string>> filterFactory) : TransactionFilterBuilder<string>(filterFactory)
    {
        private readonly Func<TransactionFilterOperation<string>, TransactionFilter<string>> _filterFactory = filterFactory;

        public TransactionFilter Contains(string value)
        {
            return _filterFactory(new ContainsTransactionFilterOperation { Value = value });
        }
        public TransactionFilter StartsWith(string value)
        {
            return _filterFactory(new StartsWithTransactionFilterOperation { Value = value });
        }
        public TransactionFilter EndsWith(string value)
        {
            return _filterFactory(new EndsWithTransactionFilterOperation { Value = value });
        }

    }

    public class TransactionFilterBuilder<T>(Func<TransactionFilterOperation<T>, TransactionFilter<T>> filterFactory)
    {
        public TransactionFilter Is(T value)
        {
            return filterFactory(new IsOneOfTransactionFilterOperation<T> { Values = [value] });
        }
        public TransactionFilter IsNot(T value)
        {
            return filterFactory(new IsNotOneOfTransactionFilterOperation<T> { Values = [value] });
        }
        public TransactionFilter IsOneOf(List<T> values)
        {
            return filterFactory(new IsOneOfTransactionFilterOperation<T> { Values = values });
        }
        public TransactionFilter IsNotOneOf(List<T> values)
        {
            return filterFactory(new IsNotOneOfTransactionFilterOperation<T> { Values = values });
        }
        public TransactionFilter IsGreaterThan(T value)
        {
            return filterFactory(new IsGreaterThanTransactionFilterOperation<T> { Value = value });
        }
        public TransactionFilter IsGreaterThanOrEqualTo(T value)
        {
            return filterFactory(new IsGreaterThanOrEqualToTransactionFilterOperation<T> { Value = value });
        }
        public TransactionFilter IsLessThan(T value)
        {
            return filterFactory(new IsLessThanTransactionFilterOperation<T> { Value = value });
        }
        public TransactionFilter IsLessThanOrEqualTo(T value)
        {
            return filterFactory(new IsLessThanOrEqualToTransactionFilterOperation<T> { Value = value });
        }
        public TransactionFilter IsNone()
        {
            return filterFactory(new IsNoneTransactionFilterOperation<T>());
        }
        public TransactionFilter IsNotNone()
        {
            return filterFactory(new IsNotNoneTransactionFilterOperation<T>());
        }
        public TransactionFilter IsNoneOr(T value)
        {
            return filterFactory(new IsNoneOrEqualToTransactionFilterOperation<T> { Value = value });
        }
    }


}
