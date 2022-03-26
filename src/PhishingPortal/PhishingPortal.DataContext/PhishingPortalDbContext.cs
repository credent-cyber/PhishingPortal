namespace PhishingPortal.DataContext
{
    using Microsoft.EntityFrameworkCore;
    using PhishingPortal.Dto.Admin;

    public class PhishingPortalDbContext : DbContext
    {
        static bool _recreateDb = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PhishingPortalDbContext(DbContextOptions options) : base(options) { 
           
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public PhishingPortalDbContext(DbContextOptions options, bool recreate = true) : this (options) {
            
            if (!_recreateDb)
                _recreateDb = recreate;

            // do seeding here
        }


        public DbSet<Tenant> Tenants { get; set; }
    }
}