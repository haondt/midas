using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Midas.UI.Models.Transactions;
using Midas.UI.Services.Transactions;
using System.Text.RegularExpressions;

namespace Midas.UI.Models.TransactionsSelect
{
    public class TransactionSelectionResult
    {
        [BindProperty(Name = "filter")]
        public List<string> Filters { get; set; } = [];
        [BindProperty(Name = "took-filtered")]
        public bool TookFiltered { get; set; } = false;
        [BindProperty(Name = "took-selected")]
        public bool TookSelected { get; set; } = true;
        [BindProperty(Name = "selection-event")]
        public string? SelectionEvent { get; set; }

        public static List<long> GetSelectedTransactions(IEnumerable<KeyValuePair<string, StringValues>> requestData) => requestData
            .Where(kvp => Regex.IsMatch(kvp.Key, "^t-[0-9]+$"))
            .Select(kvp => kvp.Key.Substring(2))
            .Select(s => long.Parse(s))
            .ToList();

        public TransactionsSelectionState GetSelectionState(IEnumerable<KeyValuePair<string, StringValues>> requestData, ITransactionFilterService filterService)
        {
            if (TookSelected)
                return new TransactionsSelectionState
                {
                    SelectNone = false,
                    SelectionEvent = SelectionEvent,
                    Filters = new List<string>
                    {
                        $"{TransactionFilterTargets.Id} {TransactionFilterOperators.IsOneOf} {string.Join(',', GetSelectedTransactions(requestData).Select(q => q.ToString()))}"
                    }
                };

            if (TookFiltered)
                return new TransactionsSelectionState
                {
                    SelectionEvent = SelectionEvent,
                    Filters = Filters,
                    SelectNone = false
                };

            return new TransactionsSelectionState
            {
                SelectionEvent = SelectionEvent,
                SelectNone = true
            };
        }
    }
}
