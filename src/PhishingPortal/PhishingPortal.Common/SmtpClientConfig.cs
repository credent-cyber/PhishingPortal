
namespace PhishingPortal.Common
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SmtpClientConfig
    {

        public bool IsSendingEnabled { get; set; } = true;
        public string Server { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string From { get; set; } = "admin@phish.credentinfotech.com";
        public string User { get; set; } = "admin@phish.credentinfotech.com";
        public string Password { get; set; } = "***********";
        public string ImageRoot { get; set; } = @"D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.UI.Blazor/wwwroot/img/";

        /// <summary>
        /// Use in case of Office365 email
        /// </summary>
        public string LicenseKey { get; set; } = "TryIt";
        public bool UseDefaultCredentials { get; set; }
        public bool EnableSsl { get; set; } = true;

        public int ConnectType { get; set; } = -1;
        public ILogger Logger { get; }

        public SmtpClientConfig(IConfiguration config, ILogger logger)
        {
            config.GetSection("SmtpConfig").Bind(this);
            Logger = logger;

            Logger.LogInformation($"SmtpConfig Configuration - start");
            Logger.LogInformation($"Server : {Server}");
            Logger.LogInformation($"Port : {Port}");
            Logger.LogInformation($"From  : {From}");
            Logger.LogInformation($"User : {User}");
            Logger.LogInformation($"ImageRoot : {ImageRoot}");
            Logger.LogInformation($"UseDefaultCredentials : {UseDefaultCredentials}");
            Logger.LogInformation($"ImageRoot : {ImageRoot}");
            Logger.LogInformation($"EnableSsl : {EnableSsl}");
            Logger.LogInformation($"ConnectType : {ConnectType}");
            Logger.LogInformation($"SmtpConfig Configuration - end");

        }
    }
}