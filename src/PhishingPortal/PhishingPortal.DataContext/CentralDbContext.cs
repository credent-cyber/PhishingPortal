namespace PhishingPortal.DataContext
{
    using Microsoft.EntityFrameworkCore;
    using PhishingPortal.Dto;

    public class CentralDbContext  : DbContext
    {
        public CentralDbContext(DbContextOptions<CentralDbContext> options)
            :base(options)
        {

        }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDomain> TenantDomain { get; set; }
        public DbSet<DemoRequestor> DemoRequestor { get; set; }
        public DbSet<AppLog> AppLogs { get; set; }
    }

}