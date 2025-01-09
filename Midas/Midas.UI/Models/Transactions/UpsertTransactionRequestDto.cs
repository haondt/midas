using Microsoft.AspNetCore.Mvc;
using Midas.Core.Models;
using Midas.UI.Shared.ModelBinders;
using System.ComponentModel.DataAnnotations;

namespace Midas.UI.Models.Transactions
{
    public class UpsertTransactionRequestDto
    {
        [BindProperty(Name = "source-account-name")]
        public string? SourceAccountName
        {
            get { return _sourceAccountName; }
            set { _sourceAccountName = value?.Trim(); }
        }
        private string? _sourceAccountName;

        [BindProperty(Name = "source-account-id")]
        [Required(AllowEmptyStrings = false)]
        public required string SourceAccount
        {
            get { return _sourceAccount; }
            set { _sourceAccount = value.Trim(); }
        }
        private string _sourceAccount = default!;

        [BindProperty(Name = "destination-account-name")]
        public string? DestinationAccountName
        {
            get { return _destinationAccountName; }
            set { _destinationAccountName = value?.Trim(); }
        }
        private string? _destinationAccountName;

        [BindProperty(Name = "destination-account-id")]
        [Required(AllowEmptyStrings = false)]
        public required string DestinationAccount
        {
            get { return _destinationAccount; }
            set { _destinationAccount = value.Trim(); }
        }
        private string _destinationAccount = default!;

        [BindProperty(Name = "description")]
        public string? Description
        {
            get { return _description; }
            set { _description = value?.Trim(); }
        }
        private string? _description = "";

        [BindProperty(Name = "amount")]
        public decimal Amount { get; set; } = 0m;

        [BindProperty(Name = "date")]
        [ModelBinder(typeof(AbsoluteDateTimeModelBinder))]
        [Required]
        public required AbsoluteDateTime Date { get; set; }

        [BindProperty(Name = "category")]
        public string? Category
        {
            get { return _category; }
            set { _category = value?.Trim(); }
        }
        private string? _category;

        [BindProperty(Name = "tags")]
        public List<string> Tags
        {
            get { return _tags; }
            set { _tags = value.Select(v => v.Trim()).Distinct().ToList(); }
        }
        private List<string> _tags = [];
    }
}
