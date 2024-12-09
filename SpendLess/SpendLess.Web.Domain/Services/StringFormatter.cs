using System.Text;
using System.Text.Json;

namespace SpendLess.Web.Domain.Services
{
    public static class StringFormatter
    {
        public static string TryPrettify(string text)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(text);
                return JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
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

        public static string FormatCurrency(decimal amount)
        {
            // TODO: appsettings the symbol
            if (amount < 0)
                return $"(${(amount * -1).ToString("N2")})";
            return $"${amount.ToString("N2")}";
        }
    }
}
