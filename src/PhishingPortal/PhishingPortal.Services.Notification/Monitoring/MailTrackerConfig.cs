namespace PhishingPortal.Services.Notification.Monitoring
{
    public class MailTrackerConfig
    {
        private const string CORRELATION_HEADER_DEFAULT_KEY = "x-cmpgn-id";

        public string CorrelationHeaderKey { get; set; } = CORRELATION_HEADER_DEFAULT_KEY;
        public string MailTrackerBlock { get; set; } = "####";
        public bool EnableEmbedTracker { get; set; } = true;
        public MailTrackerConfig(IConfiguration config)
        {
            config.GetSection("MailTracker").Bind(this);
        }
    }
}