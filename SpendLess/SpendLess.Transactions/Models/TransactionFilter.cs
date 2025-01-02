namespace SpendLess.Transactions.Models
{
    public abstract class TransactionFilter
    {
        public static TransactionFilter TransactionIdIsOneOf(List<long> ids)
        {
            return new TransactionIdIsOneOf { Ids = ids };
        }
        public static TransactionFilter MinDate(DateTime value)
        {
            return new MinDateTransactionFilter { UnixSeconds = ((DateTimeOffset)value).ToUnixTimeSeconds() };
        }
        public static TransactionFilter MaxDate(DateTime value)
        {
            return new MaxDateTransactionFilter { UnixSeconds = ((DateTimeOffset)value).ToUnixTimeSeconds() };
        }
        public static TransactionFilter ExclusiveMaxDate(DateTime value)
        {
            return new ExclusiveMaxDateTransactionFilter { UnixSeconds = ((DateTimeOffset)value).ToUnixTimeSeconds() };
        }

        public static TransactionFilter HasTag(string tag)
        {
            return new HasTagTransactionFilter { Value = tag };
        }

        public static TransactionFilter MinAmount(decimal amount)
        {
            return new MinAmountTransactionFilter { AmountCents = (long)(amount * 100) };
        }

        public static TransactionFilter MaxAmount(decimal amount)
        {
            return new MaxAmountTransactionFilter { AmountCents = (long)(amount * 100) };
        }
        public static TransactionFilter HasAmount(decimal amount)
        {
            return new HasAmountTransactionFilter { AmountCents = (long)(amount * 100) };
        }

        public static TransactionFilter HasCategory(string category)
        {
            return new HasCategoryTransactionFilter { Value = category };
        }
        public static TransactionFilter DescriptionContains(string substring)
        {
            return new DescriptionContainsTransactionFilter { Value = substring };
        }

        public static TransactionFilter EitherAccountIs(string id)
        {
            return new EitherAccountIsTransactionFilter { Id = id };
        }
        public static TransactionFilter EitherAccountIsOneOf(List<string> ids)
        {
            return new EitherAccountIsOneOfTransactionFilter { Ids = ids };
        }
    }

    public class TransactionIdIsOneOf : TransactionFilter
    {
        public required List<long> Ids { get; set; }
    }
    public class MinDateTransactionFilter : TransactionFilter
    {
        public required long UnixSeconds { get; set; }
    }
    public class MaxDateTransactionFilter : TransactionFilter
    {
        public required long UnixSeconds { get; set; }
    }
    public class ExclusiveMaxDateTransactionFilter : TransactionFilter
    {
        public required long UnixSeconds { get; set; }
    }

    public class HasTagTransactionFilter : TransactionFilter
    {
        public required string Value { get; set; }
    }
    public class HasCategoryTransactionFilter : TransactionFilter
    {
        public required string Value { get; set; }
    }

    public class DescriptionContainsTransactionFilter : TransactionFilter
    {
        public required string Value { get; set; }
    }

    public class MinAmountTransactionFilter : TransactionFilter
    {
        public required long AmountCents { get; set; }
    }
    public class MaxAmountTransactionFilter : TransactionFilter
    {
        public required long AmountCents { get; set; }
    }

    public class HasAmountTransactionFilter : TransactionFilter
    {
        public required long AmountCents { get; set; }
    }

    public class EitherAccountIsTransactionFilter : TransactionFilter
    {
        public required string Id { get; set; }
    }
    public class EitherAccountIsOneOfTransactionFilter : TransactionFilter
    {
        public required List<string> Ids { get; set; }
    }

}
