﻿using Haondt.Core.Models;

namespace SpendLess.TransactionImport.Models
{
    public class TransactionImportResultDto
    {
        public HashSet<string> Errors { get; set; } = [];
        public int TotalTransactions { get; set; }
        public bool IsSuccessful { get; set; } = true;
        public Optional<string> ImportTag { get; set; }
    }
}
