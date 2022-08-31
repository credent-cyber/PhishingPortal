using Microsoft.Graph;

namespace PhishingPortal.Server.Services.Interfaces
{
    public interface IAzActDirClientService
    {
        Task<List<User>> GetAdUsers();
        Task<Dictionary<string, string>> GetAllUserGroups();
        Task<List<User>> GetGroupMembers(string groupID);
    }
}