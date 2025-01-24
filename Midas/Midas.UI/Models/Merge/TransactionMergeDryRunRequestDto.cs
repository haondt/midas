using Microsoft.AspNetCore.Mvc;
using Midas.Core.Models;

namespace Midas.UI.Models.Merge
{
    public class TransactionMergeDryRunRequestDto
    {
        [BindProperty(Name = "filter")]
        public List<string> Filters { get; set; } = [];

        [BindProperty(Name = "transactions")]
        public string Transactions { get; set; } = "";

        [BindProperty(Name = "source-account-id")]
        public string? SourceAccountId { get; set; }
        [BindProperty(Name = "source-account-name")]
        public string? SourceAccountName { get; set; }
        [BindProperty(Name = "destination-account-id")]
        public string? DestinationAccountId { get; set; }
        [BindProperty(Name = "destination-account-name")]
        public string? DestinationAccountName { get; set; }
        [BindProperty(Name = "description")]
        public string? Description { get; set; }
        [BindProperty(Name = "amount")]
        public decimal Amount { get; set; }
        [BindProperty(Name = "timestamp")]
        public AbsoluteDateTime Timestamp { get; set; }
        [BindProperty(Name = "category")]
        public string? Category { get; set; }
        [BindProperty(Name = "tags")]
        public List<string> Tags { get; set; } = [];
    }
}
