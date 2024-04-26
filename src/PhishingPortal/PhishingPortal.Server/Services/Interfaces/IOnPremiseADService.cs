

using Microsoft.Graph;
using PhishingPortal.Dto;
using System.DirectoryServices.AccountManagement;

namespace PhishingPortal.Server.Services.Interfaces
{
    public interface IOnPremiseADService
    {
        Task<Dictionary<string, List<OnPremiseADUsers>>> GetOnPremiseADGroups();
        Task<List<OnPremiseADUsers>> GetOnPremiseUsersByADGroup(string groupName);
    }
}
