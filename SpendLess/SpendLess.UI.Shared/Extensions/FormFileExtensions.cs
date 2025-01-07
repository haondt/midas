using CsvHelper;
using Newtonsoft.Json;
using SpendLess.Core.Constants;
using SpendLess.Core.Exceptions;
using System.Globalization;

namespace SpendLess.UI.Shared.Extensions
{
    public static class FormFileExtensions
    {
        public static List<List<string>> ParseAsCsv(this IFormFile file)
        {
            var result = new List<List<string>>();
            try
            {

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                using var csv = new CsvParser(reader, CultureInfo.InvariantCulture);
                while (csv.Read())
                    if (csv.Record != null)
                        result.Add([.. csv.Record]);
            }
            catch (Exception ex)
            {
                throw new UserException($"Error reading the csv file: {ex.Message}", ex);
            }

            return result;
        }

        public static T DeserializeFromJson<T>(this IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var jsonString = reader.ReadToEnd();
                var deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, SpendLessConstants.ApiSerializerSettings)
                    ?? throw new UserException($"The JSON file could not be deserialized into type {typeof(T).Name}.");
                return deserializedObject;
            }
            catch (Exception ex) when (ex is not UserException)
            {
                throw new UserException($"Error reading or deserializing the JSON file: {ex.Message}", ex);
            }
        }
    }
}
