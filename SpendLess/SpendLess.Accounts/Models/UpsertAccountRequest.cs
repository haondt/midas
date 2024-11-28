using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.Accounts.Models
{
    public class UpsertAccountRequest
    {
        [Required(AllowEmptyStrings = false)]
        [ModelBinder(Name = "name")]
        public required string Name { get; set; }
    }
}
