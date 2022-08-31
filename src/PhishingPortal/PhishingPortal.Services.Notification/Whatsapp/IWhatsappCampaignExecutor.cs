using PhishingPortal.Services.Notification.Sms;

namespace PhishingPortal.Services.Notification.Whatsapp
{
    public interface IWhatsappCampaignExecutor : IObserver<WhatsappCampaignInfo>
    {
        IWhatsappGatewayClient WhatsAppClient { get; }
        ILogger Logger { get; }

        void Start();
        void Stop();
    }
}