namespace PhishingPortal.Services.Notification.Email.AppNotifications
{
    public interface IAppEventNotifier
    {
        Task CheckAndNotifyErrors();
    }
}