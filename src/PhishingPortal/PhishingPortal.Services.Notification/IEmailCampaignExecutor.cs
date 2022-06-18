
using PhishingPortal.Common;

namespace PhishingPortal.Services.Notification
{
    public interface IEmailCampaignExecutor : IObserver<EmailCampaignInfo>
    {
        IEmailClient EmailSender { get; }
        ILogger<EmailCampaignExecutor> Logger { get; }

        void Start();
        void Stop();
    }
}