﻿using SpendLess.Core.Models;

namespace SpendLess.Domain.Import.Models
{
    public class TransactionImportDryRunContext
    {
        public required DryRunResultDto Result { get; set; }
        public AbsoluteDateTime CurrentTimeStamp { get; set; }
        public required HashSet<string> ExistingCategories { get; set; }
        public required HashSet<string> ExistingTags { get; set; }
    }
}