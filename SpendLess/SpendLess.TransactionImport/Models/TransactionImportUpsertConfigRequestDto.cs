using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.TransactionImport.Models
{
    public class TransactionImportUpsertConfigRequestDto
    {
        public string? Account { get; set; }
        public string? Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public required string Name { get; set; }
        [ModelBinder(Name = "add-import-tag")]
        public bool AddImportTag { get; set; }
        [ModelBinder(Name = "set-default")]
        public bool SetDefault { get; set; }
    }
}
