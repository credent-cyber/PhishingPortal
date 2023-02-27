namespace PhishingPortal.OutlookAddin.Client
{
    public class BaseHttpClient
    {
        public BaseHttpClient(ILogger logger, HttpClient httpClient)
        {
            Logger = logger;
            HttpClient = httpClient;
        }

        public ILogger Logger { get; }
        public HttpClient HttpClient { get; }
    }
}
