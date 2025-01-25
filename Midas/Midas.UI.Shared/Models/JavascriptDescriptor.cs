using Haondt.Web.Services;

namespace Midas.UI.Shared.Models
{
    public class JavascriptDescriptor : IHeadEntryDescriptor
    {
        public required string Body { get; set; }
        public string Render()
        {
            return $"<script>{Body}</script>";
        }
    }
}
