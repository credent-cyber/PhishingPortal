namespace PhishingPortal.Services.Notification
{
    public class SmtpClientConfig
    {
        public string Server { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string From { get; set; } = "malay.pandey@credentinfotech.com";
        public string User { get; set; } = "malay.pandey@credentinfotech.com";
        public string Password { get; set; } = "***********";

        /// <summary>
        /// Use in case of Office365 email
        /// </summary>
        public string LicenseKey { get; set; } = "TryIt";

        public SmtpClientConfig(IConfiguration config)
        {
            config.GetSection("SmtpConfig").Bind(this);
        }
    }
}