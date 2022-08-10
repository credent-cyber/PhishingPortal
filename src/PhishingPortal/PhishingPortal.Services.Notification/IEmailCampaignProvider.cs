namespace PhishingPortal.Services.Notification
{
    public interface IEmailCampaignProvider : ICampaignProvider, IObservable<EmailCampaignInfo> { }
}