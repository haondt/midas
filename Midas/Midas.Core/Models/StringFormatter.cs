using Haondt.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            var match = Regex.Match(dateString, @"^(\d{4})-(\d{2})-(\d{2})$");
            if (!match.Success)
                return new();

            if (!int.TryParse(match.Groups[1].Value, out int year) ||
                !int.TryParse(match.Groups[2].Value, out int month) ||
                !int.TryParse(match.Groups[3].Value, out int day))
                return new();

            if (month is < 1 or > 12 || day is < 1 || day > DateTime.DaysInMonth(year, month))
                return new();

            return new(AbsoluteDateTime.Create(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local)));
        }

        public static AbsoluteDateTime ParseDate(string dateString)
        {
            var match = Regex.Match(dateString, @"^(\d{4})-(\d{2})-(\d{2})$");
            return AbsoluteDateTime.Create(new DateTime(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                0, 0, 0, DateTimeKind.Local));
        }
    }
}
