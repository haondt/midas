namespace SpendLess.Persistence.Services
{
    public class SpendLessPersistenceSettings
    {
        public required string DatabasePath { get; set; }
        public string PrimaryTableName { get; set; } = "spendless";
        public string ForeignKeyTableName { get; set; } = "foreignKeys";
        public string KvsTableName { get; set; } = "kvs";
        public int MaxKvsSearchHits { get; set; } = 5;
        public string TransactionsTableName { get; set; } = "transactions";
        public string TransactionsTagsTableName { get; set; } = "transactionsTags";
    }
}
