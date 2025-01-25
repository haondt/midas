using Haondt.Persistence.Converters;
using Midas.Core.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Midas.Core.Constants
{
    public static class MidasConstants
    {
        public const string DefaultCategory = "Uncategorized";
        public const string DefaultSupercategory = "Other";
        public const string DefaultAccount = "00000000-0000-0000-0000-000000000000";
        public const string DefaultAccountName = "Unknown";
        public const string FallbackAccountName = "No name";
        public const double MonthsPerYear = 30.4375;

        public static JsonSerializerSettings ApiSerializerSettings { get; }
        static MidasConstants()
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
            ApiSerializerSettings.Converters.Add(new AbsoluteDateTimeJsonConverter());

            PrettySerializerSettings = new JsonSerializerSettings();
            PrettySerializerSettings.TypeNameHandling = TypeNameHandling.None;
            PrettySerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            PrettySerializerSettings.Formatting = Formatting.Indented;
            PrettySerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            PrettySerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            };
            PrettySerializerSettings.Converters.Add(new GenericStorageKeyJsonConverter());
            PrettySerializerSettings.Converters.Add(new GenericOptionalJsonConverter());
            PrettySerializerSettings.Converters.Add(new AbsoluteDateTimeJsonConverter());
        }

        public static JsonSerializerSettings PrettySerializerSettings { get; }
    }
}
