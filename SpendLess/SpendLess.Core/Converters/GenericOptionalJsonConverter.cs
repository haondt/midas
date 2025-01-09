using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace SpendLess.Core.Converters
{
    public class GenericOptionalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Optional<>);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var surrogate = serializer.Deserialize<JObject>(reader);
            if (surrogate == null)
                return null;
            return OptionalSurrogate.ToOptional(surrogate, serializer);

        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var surrogate = OptionalSurrogate.FromOptional(value);
            serializer.Serialize(writer, surrogate);
        }
    }

    public class OptionalSurrogate
    {
        public object? Value { get; set; }
        public required string Type { get; set; }
        public static OptionalSurrogate FromOptional(object optional)
        {
            var optionalType = optional.GetType();
            var genericType = optionalType.GetGenericArguments()[0];
            var properties = optionalType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var hasValueProperty = properties.First(m => m.Name == "HasValue");
            var valueProperty = properties.First(m => m.Name == "Value");

            var hasValue = (bool)hasValueProperty.GetValue(optional)!;
            object? value = hasValue ? valueProperty.GetValue(optional) : null;

            var typeString = StorageKeyConvert.ConvertStorageKeyPartType(genericType, null);
            return new()
            {
                Value = value,
                Type = typeString
            };
        }

        public static object ToOptional(JObject surrogate, JsonSerializer serializer)
        {
            var valueTypeString = surrogate[nameof(Type)]!.ToString();
            var valueType = StorageKeyConvert.ConvertStorageKeyPartType(valueTypeString, null);
            var optionalType = typeof(Optional<>).MakeGenericType(valueType);

            if (surrogate.TryGetValue(nameof(Value), out var valueToken))
            {
                var valueObject = valueToken.ToObject(valueType, serializer);
                var constructor = optionalType.GetConstructor([valueType]);
                return constructor!.Invoke([valueObject]);
            }
            else
            {
                var constructor = optionalType.GetConstructor([]);
                return constructor!.Invoke([]);
            }
        }

    }
}
