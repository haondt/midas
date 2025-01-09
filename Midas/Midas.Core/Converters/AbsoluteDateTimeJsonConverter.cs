using Newtonsoft.Json;
using Midas.Core.Models;

namespace Midas.Core.Converters
{
    public class AbsoluteDateTimeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AbsoluteDateTime) || Nullable.GetUnderlyingType(objectType) == typeof(AbsoluteDateTime);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var value = serializer.Deserialize<long>(reader);
            return AbsoluteDateTime.Create(value);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var absoluteDateTime = (AbsoluteDateTime)value;
            serializer.Serialize(writer, absoluteDateTime.UnixTimeSeconds);
        }
    }
}
