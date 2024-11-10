using Haondt.Identity.StorageKey;

namespace SpendLess.Core.Extensions
{
    public static class StorageKeyExtensions
    {
        public static StorageKey WithoutFinalValue(this StorageKey storageKey)
        {
            var parts = storageKey.Parts.Select((p, i) =>
            {
                if (i == storageKey.Parts.Count - 1)
                    return new StorageKeyPart(p.Type, "");
                return p;
            }).ToList();

            return StorageKey.Create(parts);
        }

        public static string FinalValue(this StorageKey storageKey)
        {
            return storageKey.Parts[^1].Value;
        }

        public static T FinalValue<T>(this StorageKey storageKey)
        {
            return ConvertValue<T>(storageKey.FinalValue());
        }


        // TODO: move this into Haondt.Net
        private delegate bool ParseMethod<TResult>(string? value, out TResult result);
        private static T ConvertValue<T>(string? value)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            T TryParse<TParser>(ParseMethod<TParser> parseMethod)
            {
                if (!parseMethod(value, out TParser parsedValue))
                    throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
                if (parsedValue is not T castedValue)
                    throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
                return castedValue;
            }

            T FallBackTryParse()
            {
                if (targetType == typeof(Guid))
                    return TryParse<Guid>(Guid.TryParse);
                throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
            }

            return Type.GetTypeCode(targetType) switch
            {
                TypeCode.Boolean => TryParse<bool>(bool.TryParse),
                TypeCode.String => (T)(object)value!,
                TypeCode.Int16 => TryParse<int>(int.TryParse),
                TypeCode.Int32 => TryParse<int>(int.TryParse),
                TypeCode.Int64 => TryParse<int>(int.TryParse),
                TypeCode.UInt16 => TryParse<int>(int.TryParse),
                TypeCode.UInt32 => TryParse<int>(int.TryParse),
                TypeCode.UInt64 => TryParse<int>(int.TryParse),
                TypeCode.Double => TryParse<double>(double.TryParse),
                TypeCode.Decimal => TryParse<decimal>(decimal.TryParse),
                TypeCode.DateTime => TryParse<DateTime>(DateTime.TryParse),
                _ => FallBackTryParse()
            };

        }



        public static StorageKey<T> WithoutFinalValue<T>(this StorageKey<T> storageKey)
        {
            return ((StorageKey)storageKey).WithoutFinalValue().As<T>();
        }
    }
}
