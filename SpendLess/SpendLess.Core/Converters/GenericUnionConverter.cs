using Newtonsoft.Json;
using SpendLess.Core.Models;

namespace SpendLess.Core.Converters
{
    public class GenericUnionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // Handle Union<T1, T2> and Union<T1, T2>?
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Union<,>))
                return true;

            if (Nullable.GetUnderlyingType(objectType) is Type underlyingType
                && underlyingType.IsGenericType
                && underlyingType.GetGenericTypeDefinition() == typeof(Union<,>))
                return true;

            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var unionType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            var genericArgs = unionType.GetGenericArguments();
            var t1 = genericArgs[0];
            var t2 = genericArgs[1];

            // Attempt to deserialize as T1
            try
            {
                var value = serializer.Deserialize(reader, t1);
                if (value != null)
                    return Activator.CreateInstance(unionType, value);
            }
            catch (JsonSerializationException)
            {
                // If deserialization fails, continue to T2
                reader = new JsonTextReader(new StringReader(reader.Path));
            }

            // Attempt to deserialize as T2
            var fallbackValue = serializer.Deserialize(reader, t2);
            if (fallbackValue != null)
                return Activator.CreateInstance(unionType, fallbackValue);

            throw new JsonSerializationException($"Cannot deserialize value as {t1} or {t2}.");
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var unwrappedValue = value.GetType().GetMethod("Unwrap")?.Invoke(value, null);

            if (unwrappedValue == null)
                throw new JsonSerializationException("Unable to serialize Union: no active value found.");

            serializer.Serialize(writer, unwrappedValue);
        }
    }
}

