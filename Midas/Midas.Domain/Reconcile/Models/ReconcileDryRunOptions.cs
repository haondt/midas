using Midas.Persistence.Models;

namespace Midas.Domain.Reconcile.Models
{
    public class ReconcileDryRunOptions
    {
        public required List<TransactionFilter> Filters { get; set; }
        public required bool PairingMatchDescription { get; set; }
        public required bool PairingMatchDate { get; set; }
        public required int PairingDateToleranceInDays { get; set; }
        public required DescriptionJoiningStrategy JoinDescriptionStrategy { get; set; }
        public required DateJoiningStrategy JoinDateStrategy { get; set; }
        public required CategoryJoiningStrategy JoinCategoryStrategy { get; set; }
    }
}
