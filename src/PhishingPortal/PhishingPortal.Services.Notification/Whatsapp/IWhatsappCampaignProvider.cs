using PhishingPortal.Services.Notification.Sms;

namespace PhishingPortal.Services.Notification.Whatsapp
{
    public interface IWhatsappCampaignProvider : ICampaignProvider, IObservable<WhatsappCampaignInfo> { }
}