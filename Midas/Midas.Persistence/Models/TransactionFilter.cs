using Midas.Core.Models;

namespace Midas.Persistence.Models
{
    public abstract class TransactionFilter
    {
        public static TransactionFilter TransactionIdIsOneOf(List<long> ids)
        {
            return new TransactionIdIsOneOf { Ids = ids };
        }
        public static TransactionFilter MinDate(AbsoluteDateTime value)
        {
            return new MinDateTransactionFilter { UnixSeconds = value.UnixTimeSeconds };
        }
        public static TransactionFilter MaxDate(AbsoluteDateTime value)
        {
            return new MaxDateTransactionFilter { UnixSeconds = value.UnixTimeSeconds };
        }
        public static TransactionFilter ExclusiveMaxDate(AbsoluteDateTime value)
        {
            return new ExclusiveMaxDateTransactionFilter { UnixSeconds = value.UnixTimeSeconds };
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
        public static TransactionFilter CategoryIsNot(string category)
        {
            return new CategoryIsNotTransactionFilter { Value = category };
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

        public static TransactionFilter SourceAccountIsOneOf(List<string> ids)
        {
            return new SourceAccountIsOneOfTransactionFilter { Ids = ids };
        }
        public static TransactionFilter DestinationAccountIsOneOf(List<string> ids)
        {
            return new DestinationAccountIsOneOfTransactionFilter { Ids = ids };
        }
        public static TransactionFilter SourceAccountIsNotOneOf(List<string> ids)
        {
            return new SourceAccountIsNotOneOfTransactionFilter { Ids = ids };
        }
        public static TransactionFilter DestinationAccountIsNotOneOf(List<string> ids)
        {
            return new DestinationAccountIsNotOneOfTransactionFilter { Ids = ids };
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
    public class CategoryIsNotTransactionFilter : TransactionFilter
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

    public class SourceAccountIsOneOfTransactionFilter : TransactionFilter
    {
        public required List<string> Ids { get; set; }
    }
    public class DestinationAccountIsOneOfTransactionFilter : TransactionFilter
    {
        public required List<string> Ids { get; set; }
    }
    public class SourceAccountIsNotOneOfTransactionFilter : TransactionFilter
    {
        public required List<string> Ids { get; set; }
    }
    public class DestinationAccountIsNotOneOfTransactionFilter : TransactionFilter
    {
        public required List<string> Ids { get; set; }
    }

}
