using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Controllers.Api;
using PhishingPortal.Server.Services.Interfaces;
using Recipient = PhishingPortal.Dto.Recipient;
using SearchResult = System.DirectoryServices.SearchResult;
using Constants = PhishingPortal.Common.Constants;

namespace PhishingPortal.Server.Services
{
    public class OnPremiseADService : IOnPremiseADService
    {
        private readonly ILogger _logger;

        public ISettingsRepository Settings { get; }
        public TenantDbContext TenantDbCtx { get; }
        public ITenantRepository _tenantRepository { get; }

        //public string Domain="192.168.1.210";
        //public string Username= "administrator";
        //public string Password= "Pass@123";

        public OnPremiseADService(ILogger logger, TenantDbContext dbContext, ISettingsRepository settingsRepository,ITenantRepository tenantRepository)
        {
            _logger = logger;
            TenantDbCtx = dbContext;
            Settings = settingsRepository;
            _tenantRepository = tenantRepository;

        }

        public OnPremiseADService(ILogger logger, TenantDbContext dbContext)  
        {
            _logger = logger;  
            TenantDbCtx = dbContext;
           
        }

        public async Task<Dictionary<string, List<OnPremiseADUsers>>> GetOnPremiseADGroups()
        {
            Dictionary<string, List<OnPremiseADUsers>> groupsWithMembers = new Dictionary<string, List<OnPremiseADUsers>>();

            string domain = string.Empty;
            string username = string.Empty;
            string password = string.Empty;

            var tenantWithSettings = TenantDbCtx.Settings.ToList();

            if (tenantWithSettings != null)
            {
                var DomainResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Doamin);
                var UsernameResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Username);
                var PasswordResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Password);

                domain = DomainResult?.Value;
                username = UsernameResult?.Value;
                password = PasswordResult?.Value;
            }
            if (string.IsNullOrEmpty(domain) && string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                return groupsWithMembers;
            }
                string domainPath = "LDAP://" + domain;

            DirectoryEntry entry = new DirectoryEntry(domainPath, username, password);
            DirectorySearcher mySearcher = new DirectorySearcher(entry);
            mySearcher.PageSize = 1000;
            mySearcher.Filter = "(objectCategory=group)";

            try
            {
                SearchResultCollection groupResults = mySearcher.FindAll();

                foreach (SearchResult groupResult in groupResults)
                {
                    string groupName = PrintPropertyValues(groupResult, "sAMAccountName");
                    // Get group members with user details
                    List<OnPremiseADUsers> members = GetGroupMembersWithDetails(groupResult);
                    
                    groupsWithMembers.Add(groupName, members);
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            finally
            {
                entry.Close();
            }

            return groupsWithMembers;
        }
        public List<OnPremiseADUsers> GetGroupMembersWithDetails(SearchResult groupResult)
        {
            List<OnPremiseADUsers> members = new List<OnPremiseADUsers>();

            DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();
            object memberObjects = groupEntry.Invoke("Members", null);

            foreach (object member in (IEnumerable)memberObjects)
            {
                DirectoryEntry memberEntry = new DirectoryEntry(member);
                OnPremiseADUsers userDetails = GetUserDetails(memberEntry);
                if(userDetails.Email != null)
                    members.Add(userDetails);
            }

            return members;
        }

        public OnPremiseADUsers GetUserDetails(DirectoryEntry userEntry)
        {
            OnPremiseADUsers userDetails = new OnPremiseADUsers
            {
                EmployeeCode = PrintPropertyValue(userEntry, "employeeNumber"),
                Email = PrintPropertyValue(userEntry, "mail"),
                Name = PrintPropertyValue(userEntry, "displayName"),
                Mobile = PrintPropertyValue(userEntry, "telephoneNumber"),
                DateOfBirth = Convert.ToDateTime(PrintPropertyValue(userEntry, "dateOfBirth")),
                Branch = PrintPropertyValue(userEntry, "branch"),
                Department = PrintPropertyValue(userEntry, "department"),
                Address = PrintPropertyValue(userEntry, "streetAddress")
            };

            return userDetails;
        }

        public string PrintPropertyValues(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0].ToString();
            }
            else
            {
                return null;
            }
        }

        public string PrintPropertyValue(DirectoryEntry entry, string propertyName)
        {
            if (entry.Properties.Contains(propertyName) && entry.Properties[propertyName].Count > 0)
            {
                return entry.Properties[propertyName][0].ToString();
            }
            else
            {
                return null;
            }
        }

        ///////
        ///
        public async Task<List<OnPremiseADUsers>> GetOnPremiseUsersByADGroup(string groupName)
        {
            List<OnPremiseADUsers> users = new List<OnPremiseADUsers>();

            string domain = string.Empty;
            string username = string.Empty;
            string password = string.Empty;

            var tenantWithSettings = TenantDbCtx.Settings.ToList();

            if (tenantWithSettings != null)
            {
                var DomainResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Doamin);
                var UsernameResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Username);
                var PasswordResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Password);

                domain = DomainResult?.Value;
                username = UsernameResult?.Value;
                password = PasswordResult?.Value;
            }
            if (string.IsNullOrEmpty(domain) && string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                return users;
            }
            string domainPath = "LDAP://" + domain;

            DirectoryEntry entry = new DirectoryEntry(domainPath, username, password);
            DirectorySearcher groupSearcher = new DirectorySearcher(entry)
            {
                PageSize = 1000,
                Filter = $"(&(objectCategory=group)(sAMAccountName={groupName}))"
            };

            try
            {
                SearchResult groupResult = groupSearcher.FindOne();

                if (groupResult != null)
                {
                    List<OnPremiseADUsers> members = GetGroupMembersWithDetails(groupResult);
                    users.AddRange(members);
                }
                else { return null; }
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            finally
            {
                entry.Close();
            }

            return users;
        }


    }
}
