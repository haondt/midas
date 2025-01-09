using Newtonsoft.Json;

namespace Midas.Core.Extensions
{
    public static class JsonSerializerSettingsExtensions
    {
        public static JsonSerializerSettings Clone(this JsonSerializerSettings settings)
        {
            var copiedSettings = new JsonSerializerSettings
            {
                Context = settings.Context,
                Culture = settings.Culture,
                ContractResolver = settings.ContractResolver,
                ConstructorHandling = settings.ConstructorHandling,
                CheckAdditionalContent = settings.CheckAdditionalContent,
                DateFormatHandling = settings.DateFormatHandling,
                DateFormatString = settings.DateFormatString,
                DateParseHandling = settings.DateParseHandling,
                DateTimeZoneHandling = settings.DateTimeZoneHandling,
                DefaultValueHandling = settings.DefaultValueHandling,
                EqualityComparer = settings.EqualityComparer,
                FloatFormatHandling = settings.FloatFormatHandling,
                Formatting = settings.Formatting,
                FloatParseHandling = settings.FloatParseHandling,
                MaxDepth = settings.MaxDepth,
                MetadataPropertyHandling = settings.MetadataPropertyHandling,
                MissingMemberHandling = settings.MissingMemberHandling,
                NullValueHandling = settings.NullValueHandling,
                ObjectCreationHandling = settings.ObjectCreationHandling,
                PreserveReferencesHandling = settings.PreserveReferencesHandling,
                ReferenceLoopHandling = settings.ReferenceLoopHandling,
                StringEscapeHandling = settings.StringEscapeHandling,
                TraceWriter = settings.TraceWriter,
                TypeNameHandling = settings.TypeNameHandling,
                SerializationBinder = settings.SerializationBinder,
                TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling
            };
            foreach (var converter in settings.Converters)
                copiedSettings.Converters.Add(converter);
            return copiedSettings;
        }
    }
}
