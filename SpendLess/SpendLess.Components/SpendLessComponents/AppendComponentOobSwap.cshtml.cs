using System.ComponentModel;

namespace SpendLess.Components.SpendLessComponents
{
    public class AppendComponentOobSwapModel
    {
        public List<OobSwap> Items { get; set; } = [];
    }

    public class OobSwap
    {
        public required IComponent Component { get; set; }
        public required string TargetSelector { get; set; }
    }

}
