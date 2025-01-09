using System.ComponentModel.DataAnnotations;

namespace Midas.Api.Models
{
    public class ApiUpsertValueRequest
    {
        [Required(AllowEmptyStrings = false)]
        public required string Key { get; set; }
        [Required(AllowEmptyStrings = false)]
        public required string Value { get; set; }
    }
}
