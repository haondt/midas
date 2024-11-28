using Haondt.Identity.StorageKey;

namespace SpendLess.Domain.Models
{
    public class SpendLessStateDto
    {
        public static StorageKey<SpendLessStateDto> StorageKey { get; } = StorageKey<SpendLessStateDto>.Create("");
    }
}
