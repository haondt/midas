using CsvHelper;
using SpendLess.Core.Exceptions;
using System.Globalization;

namespace SpendLess.Extensions
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
    }
}
