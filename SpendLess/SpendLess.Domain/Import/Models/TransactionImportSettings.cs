namespace SpendLess.Domain.Import.Models
{
    public class TransactionImportSettings
    {
        public int StorageOperationBatchSize { get; set; } = 50;
        public int NodeRedOperationBatchSize { get; set; } = 50;

    }
}
