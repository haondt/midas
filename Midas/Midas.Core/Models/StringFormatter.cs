using Haondt.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;

namespace Midas.Core.Models
{
    public static class StringFormatter
    {
        public static string TryPrettify(string text)
        {
            try
            {
                var parsed = JToken.Parse(text);
                return parsed.ToString(Formatting.Indented);
            }
            catch
            {
                return text;
            }
        }

        public static string UrlBase64Encode(string text)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            return encoded.Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public static string UrlBase64Decode(string text)
        {
            text = text.Replace('-', '+')
                .Replace('_', '/');
            switch (text.Length % 4)
            {
                case 2: text += "=="; break;
                case 3: text += "="; break;
            }

            var decoded = Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(decoded);
        }

        public static string FormatCurrency(decimal amount, bool? useAccountingFormat = null)
        {
            if (amount < 0)
            {
                if (useAccountingFormat ?? true)
                    return $"(${(amount * -1).ToString("N2")})";
                else
                    return $"-${(amount * -1).ToString("N2")}";
            }
            return $"${amount.ToString("N2")}";
        }
        public static string FormatDate(AbsoluteDateTime absoluteDateTime)
        {
            return absoluteDateTime.LocalTime.ToString("yyyy-MM-dd");
        }

        public static Optional<AbsoluteDateTime> TryParseDate(string dateString)
        {
            dateString = dateString.Trim();
            if (string.IsNullOrEmpty(dateString))
                return new();

            if (!DateTime.TryParseExact(
                dateString,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var dt))
                return new();

            return new(AbsoluteDateTime.Create(dt));
        }

        public static AbsoluteDateTime ParseDate(string dateString)
        {
            return AbsoluteDateTime.Create(
                DateTime.ParseExact(
                    dateString,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal));
        }
    }
}
