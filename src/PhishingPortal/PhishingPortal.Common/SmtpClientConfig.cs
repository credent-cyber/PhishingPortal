
namespace PhishingPortal.Common
{
    using Microsoft.Extensions.Configuration;
    public class SmtpClientConfig
    {
        public string Server { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string From { get; set; } = "malay.pandey@credentinfotech.com";
        public string User { get; set; } = "malay.pandey@credentinfotech.com";
        public string Password { get; set; } = "***********";
        public string ImageRoot { get; set; } = @"D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.UI.Blazor/wwwroot/img/";

        /// <summary>
        /// Use in case of Office365 email
        /// </summary>
        public string LicenseKey { get; set; } = "TryIt";
        public bool UseDefaultCredentials { get; set; }
        public bool EnableSsl { get; set; } = true;

        public int ConnectType { get; set; } = -1;
        public SmtpClientConfig(IConfiguration config)
        {
            config.GetSection("SmtpConfig").Bind(this);
        }
    }
}