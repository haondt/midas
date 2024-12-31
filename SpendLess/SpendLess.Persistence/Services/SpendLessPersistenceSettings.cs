namespace SpendLess.Persistence.Services
{
    public record SpendLessPersistenceSettings
    {
        public required string DatabasePath { get; init; }
        public string PrimaryTableName { get; init; } = "spendless";
        public string ForeignKeyTableName { get; init; } = "foreignKeys";
        public string KvsTableName { get; init; } = "kvs";
        public int MaxKvsSearchHits { get; init; } = 5;
        public string TransactionsTableName { get; init; } = "transactions";
        public string TransactionsTagsTableName { get; init; } = "transactionsTags";
        public bool UseConnectionPooling { get; init; } = true;
    }
}
