using Haondt.Core.Models;

namespace SpendLess.TransactionImport.SpendLess.TransactionImport
{
    public class TransactionImportModel
    {

    }

    public class TransactionImportLayoutModel : TransactionImportModel
    {
        public TransactionImportSetupModel Setup { get; set; } = new();
    }

    public class TransactionImportSetupModel : TransactionImportModel
    {
        public const string ViewPath = "~/SpendLess.TransactionImport/Setup.cshtml";

        public bool IsSwap { get; set; } = false;
        public Dictionary<string, (string Name, Optional<string> DefaultConfigurationId)> Accounts { get; set; } = [];
        public string SelectedAccount { get; set; } = "";
        public Dictionary<string, string> Configurations { get; set; } = [];
        public string SelectedConfiguration { get; set; } = "";
    }

    public class TransactionImportEditConfigurationModel : TransactionImportModel
    {
        public const string ViewPath = "~/SpendLess.TransactionImport/EditConfiguration.cshtml";

        public bool IsCreate { get; set; } = false;
        public bool AddTagWithCurrentDateTimeToAllTransactions { get; set; } = true;
        public string Name { get; set; } = "";
        public bool IsDefaultForSelectedAccount { get; set; } = false;
        public Optional<string> SelectedAccount { get; set; } = new();
        public Optional<string> Id { get; set; } = new();
    }
}
