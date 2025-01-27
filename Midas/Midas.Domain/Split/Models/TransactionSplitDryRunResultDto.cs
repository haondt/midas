namespace Midas.Domain.Split.Models
{
    public class TransactionSplitDryRunResultDto
    {
        public Dictionary<string, string> NewAccounts { get; set; } = [];
        public List<string> NewCategories { get; set; } = new();
        public List<string> NewTags { get; set; } = [];
        public List<(string AccountName, bool IsMine, decimal Amount)> BalanceChanges { get; set; } = [];
        public List<TransactionSplit> NewTransactions { get; set; } = [];
    }

}

