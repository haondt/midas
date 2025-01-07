using Newtonsoft.Json;
using SpendLess.Charts.Converters;
using SpendLess.Core.Constants;
using SpendLess.Core.Converters;
using SpendLess.Core.Extensions;

namespace SpendLess.UI.Models.Charts
{
    public class ChartConstants
    {
        public static JsonSerializerSettings SerializerSettings { get; }
        static ChartConstants()
        {
            SerializerSettings = SpendLessConstants.ApiSerializerSettings.Clone();
            SerializerSettings.Converters.Add(new ChartCallbackJsonConverter());
            SerializerSettings.Converters.Add(new GenericUnionConverter());
        }
    }
}
