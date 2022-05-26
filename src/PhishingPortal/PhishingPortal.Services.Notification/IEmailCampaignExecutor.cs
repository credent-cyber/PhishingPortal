
namespace PhishingPortal.Services.Notification
{
    public interface IEmailCampaignExecutor : IObserver<EmailCampaignInfo>
    {
        IEmailSender EmailSender { get; }
        ILogger<EmailCampaignExecutor> Logger { get; }

        void Start();
        void Stop();
    }
}