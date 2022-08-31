namespace PhishingPortal.Services.Notification.Sms
{
    public interface ISmsCampaignProvider : ICampaignProvider, IObservable<SmsCampaignInfo> { }
}