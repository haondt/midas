using Newtonsoft.Json;
using SpendLess.Charts.Models;

namespace SpendLess.Charts.Converters
{
    public class ChartCallbackJsonConverter : JsonConverter<ChartCallback>
    {
        public override bool CanRead => false;

        public override ChartCallback? ReadJson(JsonReader reader, Type objectType, ChartCallback? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("Deserialization is not supported.");
        }

        public override void WriteJson(JsonWriter writer, ChartCallback? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteRawValue(value.Function);
        }
    }
}
