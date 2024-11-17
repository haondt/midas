using Haondt.Persistence.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpendLess.Persistence.Converters;

namespace SpendLess.Domain.Constants
{
    public static class SpendLessConstants
    {
        public const string DefaultCategory = "Uncategorized";
        public const string DefaultAccount = "00000000-0000-0000-0000-000000000000";

        public static JsonSerializerSettings ApiSerializerSettings { get; }
        static SpendLessConstants()
        {
            ApiSerializerSettings = new JsonSerializerSettings();
            ApiSerializerSettings.TypeNameHandling = TypeNameHandling.None;
            ApiSerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
            ApiSerializerSettings.Formatting = Formatting.Indented;
            ApiSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            ApiSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            ApiSerializerSettings.Converters.Add(new GenericStorageKeyJsonConverter());
            ApiSerializerSettings.Converters.Add(new GenericOptionalJsonConverter());
        }
    }
}
