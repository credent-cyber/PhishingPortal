namespace PhishingPortal.Services.Notification.Sms
{
    public interface ISmsCampaignExecutor : IObserver<SmsCampaignInfo>
    {
        ISmsGatewayClient SmsClient { get; }
        ILogger Logger { get; }

        void Start();
        void Stop();
    }
}