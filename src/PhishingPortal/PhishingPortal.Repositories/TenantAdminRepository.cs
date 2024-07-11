using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Dashboard;
using System.Linq;
using System.Text;

namespace PhishingPortal.Repositories
{

    public class TenantAdminRepository : BaseRepository, ITenantAdminRepository
    {

        public TenantAdminRepository(ILogger<TenantAdminRepository> logger, PhishingPortalDbContext2 centralDbContext, TenantAdminRepoConfig config, UserManager<PhishingPortalUser> userManager)
            : base(logger)
        {
            CentralDbContext = centralDbContext;
            Config = config;
            UserManager = userManager;
        }

        public PhishingPortalDbContext2 CentralDbContext { get; }
        public TenantAdminRepoConfig Config { get; }
        public UserManager<PhishingPortalUser> UserManager { get; }

        /// <summary>
        /// CreateTenantAsync
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public async Task<ApiResponse<Tenant>> CreateTenantAsync(Tenant tenant)
        {
            var response = new ApiResponse<Tenant>();
            var t = CentralDbContext.Tenants.FirstOrDefault(t => (t.Id > 0 && t.Id == tenant.Id)
                || t.ContactEmail.ToLower() == tenant.ContactEmail.ToLower());

            if (t != null)
            {
                //throw new Exception("Already registered");
                response.IsSuccess = false;
                response.Message = "Already registered for this domain!";
                response.Result = t;
                return response; 

            }

            tenant.UniqueId = $"{Config.DbNamePrefix}{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            tenant.ConfirmationLink = $"{tenant.GetConfirmationLink(Config.TenantConfirmBaseUrl)}";
            tenant.ConfirmationState = ConfirmationStats.Registered;

            tenant.ConfirmationExpiry = DateTime.Now.AddDays(Config.DaysToConfirm);

            tenant.CreatedOn = DateTime.Now;
            tenant.CreatedBy = Config.CreatedBy;

            var connectionString = Config.ConnectionString;

#if DEBUG
            //tenant.DatabaseOption = DbOptions.SqlLite;
#endif

            if (tenant.DatabaseOption == DbOptions.SqlLite)
            {
                connectionString = $"Data Source=./App_Data/{tenant.UniqueId}-db.db";
            }
            else
            {
                connectionString = connectionString.Replace("####", tenant.UniqueId);

            }

            var tenantSettings = new TenantData()
            {
                Key = TenantData.Keys.ConnString,
                Value = connectionString,
                CreatedBy = Config.CreatedBy,
                CreatedOn = DateTime.Now,
            };

            tenant.Settings = new List<TenantData>();
            tenant.Settings.Add(tenantSettings);

            CentralDbContext.Add(tenant);
            CentralDbContext.SaveChanges();

            var result = await CreateDatabase(tenant, tenantSettings.Value);

            //return await Task.FromResult(tenant);

            response.IsSuccess = result;
            response.Message = "Domain succesfully registered.";
            return response;
        }

        /// <summary>
        /// GetAllAsync
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<Tenant>> GetAllAsync(int pageIndex = 0, int pageSize = 10)
        {
            return await Task.FromResult(CentralDbContext.Tenants.Skip(pageIndex).Take(pageSize).ToList());
        }

        public async Task<Tenant> GetByUniqueId(string uniqueId)
        {
            return await Task.FromResult(CentralDbContext.Tenants.Where(x => x.UniqueId == uniqueId).Include(t => t.Settings).FirstOrDefault());
        }

        /// <summary>
        /// ProvisionAsync
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> ProvisionAsync(int tenantId, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Confirms registration of a new tenant by clicking the emailed link
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="hash"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Tenant> ConfirmRegistrationAsync(string uniqueId, string hash, string link)
        {
            var tenant = await Task.FromResult(CentralDbContext.Tenants.Where(x => x.UniqueId == uniqueId).FirstOrDefault());

            if (tenant == null)
                throw new InvalidOperationException("Invalid request, please contact support");

            if (tenant.ConfirmationState == ConfirmationStats.Verified)
                return tenant;

            if (tenant.ConfirmationState == ConfirmationStats.Registered && tenant.ConfirmationExpiry < DateTime.Now)
                throw new InvalidOperationException("Confirmation link is expired, please contant application support");


            if (tenant.ConfirmationExpiry > DateTime.Now && tenant.ConfirmationLink == link && tenant.ConfirmationState == ConfirmationStats.Registered)
            {
                tenant.ConfirmationState = ConfirmationStats.Verified;
                CentralDbContext.SaveChanges();

            }

            return tenant;

        }

        /// <summary>
        /// ConfirmDomain
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Tenant> ConfirmDomainAsync(DomainVerificationRequest domain)
        {
            var tenant = await Task.FromResult(CentralDbContext.Tenants.Where(x => x.UniqueId == domain.UniqueId).FirstOrDefault());

            if (tenant == null)
                throw new Exception("Tenant not found");

            if (tenant.ConfirmationState == ConfirmationStats.DomainVerified)
                return tenant;

            if (tenant.ConfirmationState == ConfirmationStats.Verified && tenant.ConfirmationExpiry < DateTime.Now)
                throw new InvalidOperationException("Confirmation link expired");
            else
            {
                if (CentralDbContext.TenantDomain.Any(o => o.Domain.ToLower() == domain.Domain.ToLower()))
                    throw new InvalidDataException("Domain already registered");

                var d = tenant.TenantDomains?.FirstOrDefault(o => o.Domain.Equals(domain.Domain, StringComparison.InvariantCultureIgnoreCase));

                if (d == null)
                {
                    d = new TenantDomain()
                    {
                        TenantId = tenant.Id,
                        Domain = domain.Domain.ToLower(),
                        DomainVerificationCode = domain.DomainVerificationCode,
                        IsDomainVerified = true,
                    };

                    CentralDbContext.Add(d);
                }
                else
                {
                    d.IsDomainVerified = true;
                    d.DomainVerificationCode = domain.DomainVerificationCode;
                }

                tenant.ConfirmationState = ConfirmationStats.DomainVerified;
                tenant.IsActive = true;
                CentralDbContext.SaveChanges();

            }

            return tenant;
        }
        public async Task<(bool, string)> DeleteTenantByUniqueId(string uniqueId)
        {
            try
            {
                var tenant = await CentralDbContext.Tenants
                    .Where(x => x.UniqueId == uniqueId)
                    .Include(t => t.Settings)
                    .Include(t => t.TenantDomains)
                    .FirstOrDefaultAsync();

                if (tenant != null)
                {

                    // Delete the associated database
                    await DeleteDatabase(tenant);

                    var tenantDataToDelete = tenant.Settings.FirstOrDefault(td => td.Value.Contains(tenant.UniqueId));
                    if (tenantDataToDelete != null)
                    {
                        tenant.Settings.Remove(tenantDataToDelete);
                        CentralDbContext.Entry(tenantDataToDelete).State = EntityState.Deleted;
                    }
                    // Remove the TenantDomain
                    var tenantDomainToDelete = CentralDbContext.TenantDomain.FirstOrDefault(z => z.TenantId == tenant.Id);
                    if (tenantDomainToDelete != null)
                    {
                        CentralDbContext.TenantDomain.Remove(tenantDomainToDelete);
                        CentralDbContext.Entry(tenantDomainToDelete).State = EntityState.Deleted;
                    }


                    var domain = tenant.ContactEmail?.Split('@').LastOrDefault();
                    if (domain != null)
                    {
                        var usersToDelete = await UserManager.Users
                            .Where(u => u.Email != null && u.Email.EndsWith("@" + domain))
                            .ToListAsync();

                        foreach (var user in usersToDelete)
                        {
                            // Delete user claims
                            var claims = await UserManager.GetClaimsAsync(user);
                            foreach (var claim in claims)
                            {
                                var result = await UserManager.RemoveClaimAsync(user, claim);
                                if (!result.Succeeded)
                                {
                                    // Handle the case where user claim removal failed
                                    throw new Exception($"Error removing user claim: {result.Errors}");
                                }
                            }

                            // Delete the user
                            var resultUserDeletion = await UserManager.DeleteAsync(user);
                            if (!resultUserDeletion.Succeeded)
                            {
                                // Handle the case where user deletion failed
                                throw new Exception($"Error deleting user: {resultUserDeletion.Errors}");
                            }
                        }
                    }






                    // Remove the tenant from DbContext
                    CentralDbContext.Tenants.Remove(tenant);

                    await CentralDbContext.SaveChangesAsync();

                    return (true, "Tenant deleted successfully.");
                }
                else
                {
                    return (false, "Tenant not found.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting tenant: {ex.Message}");
            }
        }


        private async Task<bool> DeleteDatabase(Tenant tenant)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

                if (tenant.DatabaseOption == DbOptions.SqlLite)
                {
                    optionsBuilder.UseSqlite(tenant.Settings.FirstOrDefault()?.Value);
                }
                else if (tenant.DatabaseOption == DbOptions.MySql)
                {
                    var connString = tenant.Settings.FirstOrDefault()?.Value ?? Config.ConnectionString.Replace("####", tenant.UniqueId);
                    optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(tenant.Settings.FirstOrDefault()?.Value));
                }
                else if (tenant.DatabaseOption == DbOptions.MsSql)
                {
                    optionsBuilder.UseSqlServer(tenant.Settings.FirstOrDefault()?.Value);
                }
                else
                {
                    throw new NotImplementedException("This database provider is not implemented for the tenant");
                }

                using (var db = new TenantDbContext(optionsBuilder.Options))
                {
                    await db.Database.EnsureDeletedAsync();
                }

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        private async Task<bool> CreateDatabase(Tenant tenant, string connectionString)
        {

            if (tenant != null)
            {

                // start tenant onboarding console and which will provision a new tenant.
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

                if (tenant.DatabaseOption == DbOptions.SqlLite)
                {
                    optionsBuilder.UseSqlite(connectionString);
                }
                else if (tenant.DatabaseOption == DbOptions.MySql)
                {
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                }
                else if (tenant.DatabaseOption == DbOptions.MsSql)
                {
                    optionsBuilder.UseSqlServer(connectionString);
                }
                else
                {
                    throw new NotImplementedException("This database provider is not implemented for the tenant");
                }
                // todo other provider

                TenantDbContext db;
                try
                {
                    db = new TenantDbContext(optionsBuilder.Options);
                }
                catch (Exception)
                {

                    throw;
                }
                await db.Database.EnsureCreatedAsync();

                db.SeedDefaults();
            }
            else
            {
                Logger.LogError($"$Tenant#{tenant?.UniqueId} not registered yet");
                return false;
            }


            return true;
        }

        /// <summary>
        /// Get tenant by domain
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Tenant> GetByDomain(string domain)
        {
            var tenant = CentralDbContext.Tenants.Include(o => o.TenantDomains).Include(o => o.Settings)
                .Where(o => o.TenantDomains.Any(o => o.Domain == domain.ToLower())).FirstOrDefault();

            if (tenant == null)
                throw new InvalidDataException("No tenant registered with this domain");


            return await Task.FromResult(tenant);

        }

        public async Task<string> UpsertDemoRequestor(DemoRequestor demoRequestor)
        {
            string message = string.Empty;
            try
            {
                var domain = demoRequestor.Email.Split('@').LastOrDefault(); // Get the domain part of the email
                if (!string.IsNullOrEmpty(domain))
                {
                    var requestorExists = CentralDbContext.DemoRequestor.Any(x => x.Email.Contains(domain));
                    if (requestorExists) 
                    {
                        message = "Request already Submitted!";
                    }
                }
                else
                {
                    CentralDbContext.DemoRequestor.Add(demoRequestor);
                    CentralDbContext.SaveChanges();
                    message = "Request Submitted";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return message;
        }
        public PhishingPortalDbContext PortalDbCtx { get; }
        public async Task<IEnumerable<TenantDomain>> GetDomains(int tenantId)
        {
            return await CentralDbContext.TenantDomain.Where(o => o.TenantId == tenantId).ToListAsync();
        } 

        public async Task<TenantDomain> UpsertTenantDomain(TenantDomain domain)
        {
            var existing = CentralDbContext.Find<TenantDomain>(domain.Id);

            if (CentralDbContext.TenantDomain.Any(o => o.Domain == domain.Domain))
                throw new InvalidOperationException($"{domain.Domain} already exists");

            if (!domain.Domain.IsValidDomain() || string.IsNullOrWhiteSpace(domain.DomainVerificationCode))
                throw new ArgumentException("Invalid Domain address or verification code");

            EntityEntry<TenantDomain>? result;
            
            if (existing != null)
            {
                if(existing.Domain != domain.Domain 
                    || existing.DomainVerificationCode != domain.DomainVerificationCode)
                {
                    existing.IsDomainVerified = false;
                }
                existing.DomainVerificationCode = domain.DomainVerificationCode;
                existing.Domain = domain.Domain;
                existing.ModifiedBy = domain.ModifiedBy;
                existing.ModifiedOn = domain.ModifiedOn;
                result = CentralDbContext.Update(domain);
            }

            result = CentralDbContext.Add(domain);
            await CentralDbContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<TenantDomain> VerifyDomain(TenantDomain domain)
        {
            var existing = CentralDbContext.Find<TenantDomain>(domain.Id);
            try
            {
                if (existing == null)
                {
                    throw new ArgumentException("Domain information not found");
                }
                else
                {
                    existing.IsDomainVerified = true;
                    existing.ModifiedOn = domain.ModifiedOn;
                    existing.ModifiedBy = domain.ModifiedBy;

                    CentralDbContext.Update(existing);
                    await CentralDbContext.SaveChangesAsync();
                    return existing;
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Error while doing domain verification");
            }

            return existing;
        }

        public async Task<bool> DeleteDomain(int id)
        {
            
            var existing = CentralDbContext.TenantDomain.First(x => x.Id == id);

            if (CentralDbContext.TenantDomain.Count(o => o.TenantId == existing.TenantId) == 1)
                throw new InvalidOperationException("You must keep atleast one activated domain");

            if (existing == null)
                throw new ArgumentException("Domain not found");

            CentralDbContext.TenantDomain.Remove(existing);
            await CentralDbContext.SaveChangesAsync();
            return true;
        }


        public async Task<AdminDashboardDto> GetAdminDashBoardStats()
        {
            var outcome = new AdminDashboardDto
            {
                TotalTenants = await CentralDbContext.Tenants.CountAsync(),
                ActiveTenants = await CentralDbContext.Tenants.CountAsync(x => x.IsActive == true),
                InActiveTenants = await CentralDbContext.Tenants.CountAsync(x => x.IsActive == false)
            };

            return outcome;
        }

       
    }
}