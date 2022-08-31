namespace PhishingPortal.Services.Notification.Email
{
    public interface IEmailCampaignProvider : ICampaignProvider, IObservable<EmailCampaignInfo> { }
}