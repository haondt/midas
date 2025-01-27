using Microsoft.AspNetCore.Mvc;
using Midas.Core.Constants;
using Midas.Core.Models;
using Midas.Domain.Import.Models;
using Midas.Domain.Split.Models;
using Midas.UI.Converters.Split;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Midas.UI.Models.Split
{
    public class SplitRequestDto
    {
        [BindProperty(Name = "splits"), Required]
        public required string Splits { get; set; }

        [BindProperty(Name = "source"), Required]
        public required long SourceTransactionId { get; set; }

        public List<TransactionSplit> ParseSplits()
        {
            var jsonDto = JsonConvert.DeserializeObject<List<SplitTransactionJsonDto>>(Splits)
                ?? throw new JsonSerializationException("Failed to deserialize splits.");

            return jsonDto.Select(q => new TransactionSplit
            {
                Description = q.Description,
                SourceAccount = q.SourceAccount != MidasConstants.DefaultAccount ? q.SourceAccount : Guid.NewGuid().ToString(),
                SourceAccountName = !string.IsNullOrEmpty(q.SourceAccountName?.Trim()) ? q.SourceAccountName.Trim() : MidasConstants.FallbackAccountName,
                DestinationAccountName = !string.IsNullOrEmpty(q.DestinationAccountName?.Trim()) ? q.DestinationAccountName.Trim() : MidasConstants.FallbackAccountName,
                DestinationAccount = q.DestinationAccount != MidasConstants.DefaultAccount ? q.DestinationAccount : Guid.NewGuid().ToString(),
                Amount = q.Amount,
                Date = StringFormatter.ParseDate(q.Date),
                Category = !string.IsNullOrEmpty(q.Category?.Trim()) ? q.Category.Trim() : MidasConstants.DefaultCategory,
                Tags = q.Tags ?? [],
                ImportDatum = (q.ImportDatumAccount ?? [])
                    .Zip(q.ImportDatumConfigurationSlug ?? [], q.ImportDatumValues ?? [])
                    .Select(r => new TransactionSplitImportData
                    {
                        Account = r.First,
                        ConfigurationSlug = r.Second,
                        SourceData = TransactionImportData.DestringifySourceData(r.Third)
                    })
                    .ToList()
            }).ToList();
        }
    }

    public class SplitTransactionJsonDto
    {
        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonRequired, JsonProperty("source-account-id")]
        public required string SourceAccount { get; set; }
        [JsonProperty($"source_account_name-value")]
        public string? SourceAccountName { get; set; }

        [JsonRequired, JsonProperty("destination-account-id")]
        public required string DestinationAccount { get; set; }
        [JsonProperty($"destination_account_name-value")]
        public string? DestinationAccountName { get; set; }

        [JsonRequired, JsonProperty("amount")]
        public required decimal Amount { get; set; }

        [JsonRequired, JsonProperty("date")]
        public required string Date { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("tags")]
        [JsonConverter(typeof(SafeCollectionConverter))]
        public List<string>? Tags { get; set; }

        [JsonProperty("import-datum-account")]
        [JsonConverter(typeof(SafeCollectionConverter))]
        public List<string>? ImportDatumAccount { get; set; }

        [JsonProperty("import-datum-configuration-slug")]
        [JsonConverter(typeof(SafeCollectionConverter))]
        public List<string>? ImportDatumConfigurationSlug { get; set; }

        [JsonProperty("import-datum-values")]
        [JsonConverter(typeof(SafeCollectionConverter))]
        public List<string>? ImportDatumValues { get; set; }
    }
}
