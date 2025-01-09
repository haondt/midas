using Haondt.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace Midas.Core.Models
{
    public readonly record struct Union<T1, T2> where T1 : notnull where T2 : notnull
    {
        private readonly T1 _value1 = default!;
        private readonly T2 _value2 = default!;

        private readonly bool _hasValue1 = false;
        private readonly bool _hasValue2 = false;

        public Union(T1 value)
        {
            _value1 = value;
            _hasValue1 = true;
        }
        public Union(T2 value)
        {
            _value2 = value;
            _hasValue2 = true;
        }

        public object Unwrap()
        {
            if (_hasValue1)
                return _value1;
            if (_hasValue2)
                return _value2;
            throw new InvalidOperationException("Union has no values.");
        }

        public bool Is<T>([MaybeNullWhen(false)] out T value) where T : notnull
        {
            if (typeof(T) == typeof(T1))
            {
                if (_hasValue1)
                {
                    value = (T)(object)_value1;
                    return true;
                }
            }
            else if (typeof(T) == typeof(T2))
            {
                if (_hasValue2)
                {
                    value = (T)(object)_value2;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public Optional<T> As<T>() where T : notnull
        {
            if (Is<T>(out var value))
                return value;
            return new();
        }

        public static implicit operator Union<T1, T2>(T1 value) => new Union<T1, T2>(value);
        public static implicit operator Union<T1, T2>(T2 value) => new Union<T1, T2>(value);
        public static explicit operator T2(Union<T1, T2> union) => union._hasValue2 ? union._value2 : throw new InvalidOperationException($"Union is not of type {typeof(T2)}");
        public static explicit operator T1(Union<T1, T2> union) => union._hasValue1 ? union._value1 : throw new InvalidOperationException($"Union is not of type {typeof(T1)}");
    }


}
