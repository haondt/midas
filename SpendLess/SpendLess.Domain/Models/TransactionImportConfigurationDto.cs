namespace SpendLess.Domain.Models
{
    public class TransactionImportConfigurationDto
    {
        public bool AddImportTag { get; set; } = true;
        public string Name { get; set; } = "";
    }
}
