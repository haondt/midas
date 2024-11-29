namespace SpendLess.Domain.Models
{
    public class AccountDto
    {
        public required string Name { get; set; }
        public required decimal Balance { get; set; }
        public bool IsWatched { get; set; } = false;
    }
}
