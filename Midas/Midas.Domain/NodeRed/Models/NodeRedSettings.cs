namespace Midas.Domain.NodeRed.Models
{
    public class NodeRedSettings
    {
        public required string BaseUrl { get; set; }
        public required string ClientUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 1;
    }
}
