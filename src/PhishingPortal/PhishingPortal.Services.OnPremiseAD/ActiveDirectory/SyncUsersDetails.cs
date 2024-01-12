using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Services.OnPremiseAD.Helper;
using System.DirectoryServices;
using SearchResult = System.DirectoryServices.SearchResult;

namespace PhishingPortal.Services.OnPremiseAD.ActiveDirectory
{
    public class SyncUsersDetails : ISyncUsersDetails
    {
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<SyncUsersDetails> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }
        public SyncUsersDetails(ILogger<SyncUsersDetails> logger,
           IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            Tenant = tenant;
            ConnManager = connManager;
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
        }
        public async Task Sync(CancellationToken stopppingToken)
        {
            await Task.Run(async () =>
            {

                try
                {
                        var dbContext = ConnManager.GetContext(Tenant.UniqueId);
                    
                        string domain = string.Empty;
                        string username = string.Empty;
                        string password = string.Empty;
                        var tenantWithSettings = dbContext.Settings.ToList();

                        if (tenantWithSettings != null)
                        {
                            var DomainResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Doamin);
                            var UsernameResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Username);
                            var PasswordResult = tenantWithSettings.FirstOrDefault(x => x.Key == Constants.Keys.OnPromiseAD_Password);

                            domain = DomainResult?.Value;
                            username = UsernameResult?.Value;
                            password = PasswordResult?.Value;
                        }
                        if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            using (var entry = new DirectoryEntry($"LDAP://{domain}", username, password))
                            using (var mySearcher = new DirectorySearcher(entry))
                            {
                                mySearcher.PageSize = 1000;
                                mySearcher.Filter = "(&(objectCategory=person)(objectClass=user))";
                                try
                                {
                                    SearchResultCollection resultCollection = mySearcher.FindAll();

                                    var existingEmails = new HashSet<string>(dbContext.OnPremiseADUsers.Select(r => r.Email));

                                    foreach (SearchResult result in resultCollection)
                                    {
                                        string mail = GetPropertyValue(result, "mail");
                                        if (string.IsNullOrEmpty(mail) || mail.Contains("N/A"))
                                            continue;
                                        if (existingEmails.Contains(mail))
                                        {
                                            var onPremiseADUser = dbContext.OnPremiseADUsers.SingleOrDefault(r => r.Email == mail);
                                            if (onPremiseADUser != null)
                                            {
                                                UpdateUserFromAD(result, onPremiseADUser);
                                            }
                                        }
                                        else
                                        {
                                            AddNewUserFromAD(result, dbContext, existingEmails);
                                        }
                                    }

                                    await dbContext.SaveChangesAsync(stopppingToken);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, "Error while processing On-Premises AD users.");
                                }
                            }
                          
                        }
                      
                    

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }
            });
        }


        private void UpdateUserFromAD(SearchResult result, OnPremiseADUsers existingUser)
        {
            existingUser.EmployeeCode = GetPropertyValue(result, "employeeNumber");
            existingUser.Name = $"{GetPropertyValue(result, "givenName")} {GetPropertyValue(result, "sn")}";
            existingUser.Mobile = GetPropertyValue(result, "telephoneNumber");
            existingUser.WhatsAppNo = GetPropertyValue(result, "telephoneNumber");
            existingUser.ModifiedOn = DateTime.Now;

            var DOB = GetPropertyValue(result, "birthdate");
            DateTime? parsedDateOfBirth = null;

            if (DOB.Contains("N/A"))
            {
                parsedDateOfBirth = DateTime.MinValue;
            }
            else
            {
                parsedDateOfBirth = Convert.ToDateTime(DOB);
            }

            existingUser.DateOfBirth = parsedDateOfBirth;
            existingUser.Status = GetPropertyValue(result, "status");
            existingUser.LastWorkingDate = GetPropertyValue(result, "lastWorkingDate");
            existingUser.Address = GetPropertyValue(result, "address");
            existingUser.Department = GetPropertyValue(result, "department");
            existingUser.Branch = GetPropertyValue(result, "branch");

            Logger.LogInformation($"Updated user {existingUser.Email} from AD on {DateTime.Now}");
        }


        private void AddNewUserFromAD(SearchResult result, TenantDbContext dbContext, HashSet<string> existingEmails)
        {
            string mail = GetPropertyValue(result, "mail");
            string sAMAccountName = GetPropertyValue(result, "sAMAccountName");
            string givenName = GetPropertyValue(result, "givenName");
            string sn = GetPropertyValue(result, "sn");
            string telephoneNumber = GetPropertyValue(result, "telephoneNumber");
            string title = GetPropertyValue(result, "title");
            string department = GetPropertyValue(result, "department");
            string streetAddress = GetPropertyValue(result, "streetAddress");
            string employeeNumber = GetPropertyValue(result, "employeeNumber");
            string dateOfBirth = GetPropertyValue(result, "birthdate");

            DateTime? parsedDateOfBirth = null;

            if (dateOfBirth.Contains("N/A"))
            {
                parsedDateOfBirth = DateTime.MinValue;
            }

            string status = GetPropertyValue(result, "status");
            string lastWorkingDate = GetPropertyValue(result, "lastWorkingDate");
            string address = GetPropertyValue(result, "address");
            string departmentValue = GetPropertyValue(result, "department");
            string branch = GetPropertyValue(result, "branch");

            var newUser = new OnPremiseADUsers
            {
                EmployeeCode = employeeNumber,
                Name = $"{givenName} {sn}",
                Email = mail,
                Mobile = telephoneNumber,
                WhatsAppNo = telephoneNumber,
                DateOfBirth = parsedDateOfBirth,
                IsActive = true,
                Status = status,
                LastWorkingDate = lastWorkingDate,
                Address = address,
                Department = departmentValue,
                Branch = branch,
                CreatedOn = DateTime.Now,
            };

            dbContext.OnPremiseADUsers.Add(newUser);
            existingEmails.Add(mail);

            Logger.LogInformation($"Added new user {newUser.Email} from AD on {DateTime.Now}");
        }


        private string GetPropertyValue(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0].ToString();
            }
            else
            {
                return "N/A";
            }
        }

    }
}
