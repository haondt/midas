using Haondt.Core.Models;

namespace Midas.Domain.Admin.Models
{
    public class TakeoutResult
    {
        public HashSet<string> Errors { get; set; } = [];
        public bool IsSuccessful { get; set; } = true;
        public Optional<string> ZipPath { get; set; }
    }
}