using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PhishingPortal.Common
{
    public class AppConfig
    {

        public AppConfig()
        {
            // TODO: load from config
        }

        public static int ImportRecipientCsvMinColumns  = 8;
        public static string EmailRegex  = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
    }
}
