using Haondt.Core.Models;

namespace SpendLess.Admin.Services
{
    public class TakeoutResult
    {
        public HashSet<string> Errors { get; set; } = [];
        public bool IsSuccessful { get; set; } = true;
        public Optional<string> ZipPath { get; set; }
    }
}