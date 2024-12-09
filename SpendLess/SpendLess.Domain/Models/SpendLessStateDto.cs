using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Models
{
    public class SpendLessStateDto
    {
        public static StorageKey<SpendLessStateDto> StorageKey { get; } = StorageKey<SpendLessStateDto>.Create("");

        public HashSet<string> Tags { get; set; } = [];
        public HashSet<string> Categories { get; set; } = [];
    }
}
