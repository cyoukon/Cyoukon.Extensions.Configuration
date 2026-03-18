namespace Cyoukon.Extensions.Configuration.Abstractions
{
    public class WebhookResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public bool ConfigurationChanged { get; set; }
        public string[]? ChangedFiles { get; set; }
    }
}
