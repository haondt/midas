using Newtonsoft.Json;
using Midas.Charts.Converters;
using Midas.Core.Constants;
using Midas.Core.Converters;
using Midas.Core.Extensions;

namespace Midas.UI.Models.Charts
{
    public class ChartConstants
    {
        public static JsonSerializerSettings SerializerSettings { get; }
        static ChartConstants()
        {
            SerializerSettings = MidasConstants.ApiSerializerSettings.Clone();
            SerializerSettings.Converters.Add(new ChartCallbackJsonConverter());
            SerializerSettings.Converters.Add(new GenericUnionConverter());
        }
    }
}
