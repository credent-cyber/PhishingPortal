namespace PhishingPortal.Repositories
{
    public interface IAppNotificationRepository
    {
        void LogCriticalError(Exception ex, string message);
    }
}