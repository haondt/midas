namespace Midas.Persistence.Models
{
    public abstract class TransactionFilterOperation
    {

    }
    public abstract class TransactionFilterOperation<T> : TransactionFilterOperation
    {

    }

    public abstract class NoOperandTransactionFilterOperation<T> : TransactionFilterOperation<T>
    {
    }

    public abstract class SingleOperandTransactionFilterOperation<T> : TransactionFilterOperation<T>
    {
        public required T Value { get; set; }
    }

    public abstract class ListOperandTransactionFilterOperation<T> : TransactionFilterOperation<T>
    {
        public required List<T> Values { get; set; }
    }

    public class IsGreaterThanTransactionFilterOperation<T> : SingleOperandTransactionFilterOperation<T> { }
    public class IsGreaterThanOrEqualToTransactionFilterOperation<T> : SingleOperandTransactionFilterOperation<T> { }
    public class IsLessThanTransactionFilterOperation<T> : SingleOperandTransactionFilterOperation<T> { }
    public class IsLessThanOrEqualToTransactionFilterOperation<T> : SingleOperandTransactionFilterOperation<T> { }
    public class IsNoneTransactionFilterOperation<T> : NoOperandTransactionFilterOperation<T> { }
    public class IsNoneOrEqualToTransactionFilterOperation<T> : SingleOperandTransactionFilterOperation<T> { }
    public class IsNotNoneTransactionFilterOperation<T> : NoOperandTransactionFilterOperation<T> { }
    public class ContainsTransactionFilterOperation : SingleOperandTransactionFilterOperation<string> { }
    public class StartsWithTransactionFilterOperation : SingleOperandTransactionFilterOperation<string> { }
    public class EndsWithTransactionFilterOperation : SingleOperandTransactionFilterOperation<string> { }
    public class IsOneOfTransactionFilterOperation<T> : ListOperandTransactionFilterOperation<T> { }
    public class IsNotOneOfTransactionFilterOperation<T> : ListOperandTransactionFilterOperation<T> { }

}
