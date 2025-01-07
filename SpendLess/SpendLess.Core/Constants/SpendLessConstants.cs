using Haondt.Persistence.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpendLess.Core.Converters;

namespace SpendLess.Core.Constants
{
    public static class SpendLessConstants
    {
        public const string DefaultCategory = "Uncategorized";
        public const string DefaultAccount = "00000000-0000-0000-0000-000000000000";
        public const string DefaultAccountName = "Unknown";
        public const string FallbackAccountName = "No name";

        public static JsonSerializerSettings ApiSerializerSettings { get; }
        static SpendLessConstants()
        {
            ApiSerializerSettings = new JsonSerializerSettings();
            ApiSerializerSettings.TypeNameHandling = TypeNameHandling.None;
            ApiSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            ApiSerializerSettings.Formatting = Formatting.None;
            ApiSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            ApiSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            };
            ApiSerializerSettings.Converters.Add(new GenericStorageKeyJsonConverter());
            ApiSerializerSettings.Converters.Add(new GenericOptionalJsonConverter());

            PrettySerializerSettings = new JsonSerializerSettings();
            PrettySerializerSettings.TypeNameHandling = TypeNameHandling.None;
            PrettySerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            PrettySerializerSettings.Formatting = Formatting.Indented;
            PrettySerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            PrettySerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            PrettySerializerSettings.Converters.Add(new GenericStorageKeyJsonConverter());
            PrettySerializerSettings.Converters.Add(new GenericOptionalJsonConverter());
        }

        public static JsonSerializerSettings PrettySerializerSettings { get; }
    }
}
