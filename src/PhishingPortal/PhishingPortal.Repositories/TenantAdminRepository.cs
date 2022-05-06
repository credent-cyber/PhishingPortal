using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{
    public class TenantAdminRepository : BaseRepository, ITenantAdminRepository
    {

        public TenantAdminRepository(ILogger<TenantAdminRepository> logger, PhishingPortalDbContext centralDbContext)
            : base(logger)
        {
            CentralDbContext = centralDbContext;
  
        }

        public PhishingPortalDbContext CentralDbContext { get; }


        /// <summary>
        /// CreateTenantAsync
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {

            tenant.UniqueId = $"ClientDb-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            tenant.CreatedOn = DateTime.Now;
            tenant.CreatedBy = "System";
            tenant.DomainVerificationCode = Guid.NewGuid().ToString();
            tenant.IsDomainVerified = "F";

            CentralDbContext.Add(tenant);
            CentralDbContext.SaveChanges();

            
            return await Task.FromResult(tenant);
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

        /// <summary>
        /// ProvisionAsync
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> ProvisionAsync(int tenantId, string connectionString)
        {
            
            var tenant = await CentralDbContext.Tenants.SingleAsync(tenant => tenant.Id == tenantId);

            if (tenant != null)
            {
                if(tenant.DatabaseOption == DbOptions.SqlLite)
                {
                    connectionString = $"Data Source=./App_Data/{tenant.UniqueId}.db";
                }
                else
                {
                    connectionString = ConstructConnString(connectionString, tenant.DatabaseOption);
                }

                var tenantSettings = new TenantData()
                {
                    Key = "CONN_STR",
                    Value = connectionString,
                    CreatedBy = "System",
                    CreatedOn = DateTime.Now,
                };

                CentralDbContext.Add(tenantSettings);
                CentralDbContext.SaveChanges();

                // start tenant onboarding console and which will provision a new tenant.
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

                if (tenant.DatabaseOption == DbOptions.SqlLite)
                {
                    optionsBuilder.UseSqlite(tenantSettings.Value);
                }
              
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
            }
            else
            {
                Logger.LogError($"$Tenant#{tenantId} not registered yet");
                return false;
            }
                

            return true;
        }

        private string ConstructConnString(string connString, DbOptions options)
        {
            // TODO: reconstruct connection string from the input string
            return connString;
        }
    }
} 