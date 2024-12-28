namespace SpendLess.Accounts.Models
{
    public class AccountDetails
    {
        public required string Name { get; set; }
        public required bool IsIncludedInNetWorth { get; set; }
        public decimal Balance { get; set; }
    }
}