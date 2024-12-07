using System.ComponentModel.DataAnnotations;

namespace SpendLess.Kvs.Models
{
    public class ApiUpsertValueRequest
    {
        [Required(AllowEmptyStrings = false)]
        public required string Key { get; set; }
        [Required(AllowEmptyStrings = false)]
        public required string Value { get; set; }
    }
}
