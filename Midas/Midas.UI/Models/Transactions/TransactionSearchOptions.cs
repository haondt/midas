using Microsoft.AspNetCore.Mvc;

namespace Midas.UI.Models.Transactions
{
    public class TransactionSearchOptions
    {
        public const string HideCheckboxesParameter = "ts-hide-checkboxes";
        [BindProperty(Name = HideCheckboxesParameter)]
        public bool HideCheckboxes { get; set; } = false;

        public const string HideActionsParameter = "ts-hide-actions";
        [BindProperty(Name = HideActionsParameter)]
        public bool HideActions { get; set; } = false;

        public const string LinkTargetParameter = "ts-link-target";
        [BindProperty(Name = LinkTargetParameter)]
        public TransactionLinkTarget LinkTarget { get; set; } = TransactionLinkTarget.Here;

        public const string LinkCustomTargetParameter = "ts-link-custom-target";
        [BindProperty(Name = LinkCustomTargetParameter)]
        public string? LinkCustomTarget { get; set; }

        public const string HideLineItemFilterButtonsParameter = "ts-hide-line-item-filter-buttons";
        [BindProperty(Name = HideLineItemFilterButtonsParameter)]
        public bool HideLineItemFilterButtons { get; set; } = false;

        public const string HideAccountIdsParameter = "ts-hide-account-ids";
        [BindProperty(Name = HideAccountIdsParameter)]
        public bool HideAccountIds { get; set; } = false;

        public const string HideDetailsParameter = "ts-hide-details";
        [BindProperty(Name = HideDetailsParameter)]
        public bool HideDetails { get; set; } = false;
    }

    public enum TransactionLinkTarget
    {
        Here,
        Blank,
        Custom,
        None
    }
}
