using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Admin;

namespace PhishingPortal.DataContext
{
    public class TenantDbContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TenantDbContext(DbContextOptions options) : base(options) {

            Database.EnsureCreated();
            // seed here
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public DbSet<Tenant> TenantInfo { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }

    }
}
