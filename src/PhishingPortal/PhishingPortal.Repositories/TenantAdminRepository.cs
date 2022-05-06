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


        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {

            tenant.UniqueId = $"ClientDb-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            tenant.CreatedOn = DateTime.Now;
            tenant.CreatedBy = "System";
            tenant.DomainVerificationCode = Guid.NewGuid().ToString();
            tenant.IsDomainVerified = "F";

            CentralDbContext.Add(tenant);
            CentralDbContext.SaveChanges();

            //if (tenant.Id > 0)
            //{
           
            //    // start tenant onboarding console and which will provision a new tenant.
            //    var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

            //    if (tenant.DatabaseOption == DbOptions.SqlLite)
            //    {
            //        optionsBuilder.UseSqlite($"Data Source=./App_Data/{tenant.UniqueId}.db");
            //    }
            //    else
            //    {
            //        // TODO: provision other db options
            //    }
            //    TenantDbContext db;
            //    try
            //    {
            //        db = new TenantDbContext(optionsBuilder.Options);
            //    }
            //    catch (Exception)
            //    {

            //        throw;
            //    }
            //    await db.Database.EnsureCreatedAsync();


            //}
            return await Task.FromResult(tenant);
        }


    }
} 