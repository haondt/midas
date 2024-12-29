using Newtonsoft.Json;
using SpendLess.Charts.Converters;
using SpendLess.Domain.Constants;
using SpendLess.Domain.Extensions;

namespace SpendLess.Charts.Constants
{
    public class ChartConstants
    {
        public static JsonSerializerSettings SerializerSettings { get; }
        static ChartConstants()
        {
            SerializerSettings = SpendLessConstants.ApiSerializerSettings.Clone();
            SerializerSettings.Converters.Add(new ChartCallbackJsonConverter());
        }
    }
}
