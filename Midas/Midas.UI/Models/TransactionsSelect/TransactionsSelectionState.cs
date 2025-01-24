using Microsoft.AspNetCore.Mvc;

namespace Midas.UI.Models.TransactionsSelect
{
    public class TransactionsSelectionState
    {
        [BindProperty(Name = "filter")]
        public List<string> Filters { get; set; } = [];

        [BindProperty(Name = "has-none-selected")]
        public bool SelectNone { get; set; } = true;

        [BindProperty(Name = "selection-event")]
        public string? SelectionEvent { get; set; }
    }
}
