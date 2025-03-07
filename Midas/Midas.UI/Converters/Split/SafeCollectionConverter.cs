﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Midas.UI.Converters.Split
{
    // I "borrowed" this from https://stackoverflow.com/a/58708881
    public class SafeCollectionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            //This not works for Populate (on existingValue)
            return serializer.Deserialize<JToken>(reader)!.ToObjectCollectionSafe(objectType, serializer);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
