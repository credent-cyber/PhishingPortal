namespace PhishingPortal.Repositories
{
    public interface ISettingsRepository
    {
        Task<T> GetSetting<T>(string key);
        Task<Dictionary<string, string>> GetSettings();
        Task<Dictionary<string, string>> UpsertSettings(Dictionary<string, string> settings, string user);
    }
}