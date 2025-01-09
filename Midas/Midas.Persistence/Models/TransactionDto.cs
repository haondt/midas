using Midas.Core.Constants;
using Midas.Core.Models;

namespace Midas.Persistence.Models
{
    public class TransactionDto
    {
        public required decimal Amount { get; set; }
        public HashSet<string> Tags { get; set; } = [];
        public string Category { get; set; } = MidasConstants.DefaultCategory;
        public string SourceAccount { get; set; } = MidasConstants.DefaultAccount;
        public string DestinationAccount { get; set; } = MidasConstants.DefaultAccount;
        public string Description { get; set; } = "";
        public required AbsoluteDateTime TimeStamp { get; set; }
    }
}
