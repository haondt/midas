using Haondt.Core.Models;

namespace Midas.Core.Extensions
{
    public static class ResultExtensions
    {
        public static Optional<T> AsOptional<T, TReason>(this DetailedResult<T, TReason> result) where T : notnull
        {
            return result.IsSuccessful ? new(result.Value) : new();
        }

        public static T Or<T, TReason>(this DetailedResult<T, TReason> result, T defaultValue) where T : notnull
        {
            if (result.IsSuccessful)
                return result.Value;
            return defaultValue;
        }
        public static T Or<T, TReason>(this DetailedResult<T, TReason> result, Func<T> defaultValueFactory) where T : notnull
        {
            if (result.IsSuccessful)
                return result.Value;
            return defaultValueFactory();
        }
        public static Optional<T> AsOptional<T>(this Result<T> result) where T : notnull
        {
            return result.IsSuccessful ? new(result.Value) : new();
        }

        public static T Or<T>(this Result<T> result, T defaultValue) where T : notnull
        {
            if (result.IsSuccessful)
                return result.Value;
            return defaultValue;
        }
        public static T Or<T>(this Result<T> result, Func<T> defaultValueFactory) where T : notnull
        {
            if (result.IsSuccessful)
                return result.Value;
            return defaultValueFactory();
        }
    }
}
