using SpendLess.Domain.Constants;

namespace SpendLess.TransactionImport.Models
{
    public class TransactionImportWarning
    {

        public const string MissingSourceAccount = "no_source";
        public const string MissingDestinationAccount = "no_destination";
        public const string CreatingAccountWithSameNameAsExisting = "create_with_same_name_as_existing";
        public const string MissingCategory = "no_category";
        public const string SourceDataHashExists = "source_data_hash_exists";

        public static string GetDescription(string value) => value switch
        {
            MissingSourceAccount => $"No source account was set. The default \"{SpendLessConstants.DefaultAccountName}\" account will be used.",
            MissingDestinationAccount => $"No source destination was set. The default \"{SpendLessConstants.DefaultAccountName}\" account will be used.",
            CreatingAccountWithSameNameAsExisting => "The account being created has the same name as an existing account.",
            MissingCategory => $"No category was set. The default \"{SpendLessConstants.DefaultCategory}\" category will be used.",
            SourceDataHashExists => $"There is one or more existing transactions that were generated from identical import data.",
            _ => throw new ArgumentException($"value \"{value}\" not known.")
        };
    }
}
