using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{

    public class TenantAdminRepository : BaseRepository, ITenantAdminRepository
    {

        public TenantAdminRepository(ILogger<TenantAdminRepository> logger, PhishingPortalDbContext centralDbContext, TenantAdminRepoConfig config)
            : base(logger)
        {
            CentralDbContext = centralDbContext;
            Config = config;
        }

        public PhishingPortalDbContext CentralDbContext { get; }
        public TenantAdminRepoConfig Config { get; }


        /// <summary>
        /// CreateTenantAsync
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {

            tenant.UniqueId = $"{Config.DbNamePrefix}{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            tenant.ConfirmationLink = $"{Config.TenantConfirmBaseUrl}{tenant.GetConfirmationLink(tenant.ConfirmationLink)}";
            tenant.ConfirmationState = ConfirmationStats.Registered;
            tenant.ConfirmationExpiry = DateTime.Now.AddDays(Config.DaysToConfirm);

            tenant.CreatedOn = DateTime.Now;
            tenant.CreatedBy = Config.CreatedBy;

            var connectionString = Config.ConnectionString;

#if DEBUG
            tenant.DatabaseOption = DbOptions.SqlLite;
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

        public async Task<Tenant> GetByUniqueId(string uniqueId)
        {
            return await Task.FromResult(CentralDbContext.Tenants.Where(x => x.UniqueId == uniqueId).FirstOrDefault());
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
            }
            else
            {
                Logger.LogError($"$Tenant#{tenant?.UniqueId} not registered yet");
                return false;
            }


            return true;
        }
       
    }
} 