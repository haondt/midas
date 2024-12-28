namespace SpendLess.Web.Domain.Models
{
    public class NavigationSection : NavigationItem
    {
        public required string Label { get; set; }
        public List<NavigationItem> Children { get; set; } = [];
    }
}
