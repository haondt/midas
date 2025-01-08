using Newtonsoft.Json;
using SpendLess.Core.Models;

namespace SpendLess.Core.Converters
{
    public class AbsoluteDateTimeJsonConverter : JsonConverter<AbsoluteDateTime>
    {
        public override AbsoluteDateTime ReadJson(JsonReader reader, Type objectType, AbsoluteDateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<long>(reader);
            return AbsoluteDateTime.Create(value);
        }

        public override void WriteJson(JsonWriter writer, AbsoluteDateTime value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.UnixTimeSeconds);
        }
    }
}
