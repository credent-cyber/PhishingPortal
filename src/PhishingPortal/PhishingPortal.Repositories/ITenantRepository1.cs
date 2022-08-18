namespace PhishingPortal.Repositories
{
    public interface ISettingsRepository
    {
        Task<T> GetSetting<T>(string key);
    }
}