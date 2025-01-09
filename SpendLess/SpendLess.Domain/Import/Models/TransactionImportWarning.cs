using SpendLess.Core.Constants;

namespace SpendLess.Domain.Import.Models
{
    public class TransactionImportWarning
    {

        public const string MissingSourceAccount = "no_source";
        public const string MissingDestinationAccount = "no_destination";
        public const string CreatingAccountWithSameNameAsExisting = "create_with_same_name_as_existing";
        public const string MissingCategory = "no_category";
        public const string SourceDataHashExists = "source_data_hash_exists";
        public const string WillUpdateExisting = "will_update_existing";
        public const string WasMerged = "was_merged";

        public static string GetDescription(string value) => value switch
        {
            MissingSourceAccount => $"No source account was set. The default \"{SpendLessConstants.DefaultAccountName}\" account will be used.",
            MissingDestinationAccount => $"No source destination was set. The default \"{SpendLessConstants.DefaultAccountName}\" account will be used.",
            CreatingAccountWithSameNameAsExisting => "The account being created has the same name as an existing account.",
            MissingCategory => $"No category was set. The default \"{SpendLessConstants.DefaultCategory}\" category will be used.",
            SourceDataHashExists => $"There is one or more existing transactions that were generated from identical import data.",
            WillUpdateExisting => $"One or more existing transactions will be updated.",
            WasMerged => $"One or more existing transactions were merged into a single transaction, and will be split back into their original components.",
            _ => throw new ArgumentException($"value \"{value}\" not known.")
        };
    }
}
