using Midas.Charts.Converters;
using Midas.Core.Constants;
using Midas.Core.Extensions;
using Newtonsoft.Json;

namespace Midas.UI.Models.Charts
{
    public class ChartConstants
    {
        public static JsonSerializerSettings SerializerSettings { get; }
        static ChartConstants()
        {
            SerializerSettings = MidasConstants.ApiSerializerSettings.Clone();
            SerializerSettings.Converters.Add(new ChartCallbackJsonConverter());
        }
    }
}
