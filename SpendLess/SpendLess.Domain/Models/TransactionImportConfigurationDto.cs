namespace SpendLess.Domain.Models
{
    public class TransactionImportConfigurationDto
    {
        public bool AddTagWithCurrentDateTimeToAllTransactions { get; set; } = true;
        public string Name { get; set; } = "";
    }
}
