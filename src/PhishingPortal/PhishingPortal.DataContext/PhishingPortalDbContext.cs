using PhishingPortal.DataContext;

namespace PhishingPortal.DataContext
{
    using Duende.IdentityServer.EntityFramework.Options;
    using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using PhishingPortal.Domain;
    using PhishingPortal.Dto;

   
    public class PhishingPortalDbContext : ApiAuthorizationDbContext<PhishingPortalUser>
    {
        static bool _recreateDb = false;
        private DbContextOptionsBuilder<PhishingPortalDbContext> dbCtxOptions;
        private DbContextOptions<PhishingPortalDbContext> options;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PhishingPortalDbContext(DbContextOptions<PhishingPortalDbContext> options,
            IOptions<OperationalStoreOptions> operationStoreOptions) : base(options, operationStoreOptions)
        {

        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        //public PhishingPortalDbContext(DbContextOptions options, bool recreate = true) : this(options)
        //{

        //    if (!_recreateDb)
        //        _recreateDb = recreate;

        //    // do seeding here
        //}


        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDomain> TenantDomain { get; set; }
        public DbSet<DemoRequestor> DemoRequestor { get; set; }



    }


    public class PhishingPortalDbContext2 : IdentityDbContext<PhishingPortalUser>
    {
        static bool _recreateDb = false;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PhishingPortalDbContext2(DbContextOptions<PhishingPortalDbContext2> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDomain> TenantDomain { get; set; }
        public DbSet<DemoRequestor> DemoRequestor { get; set; }
        public DbSet<AppLog> AppLog { get; set; }
      
       
    }
}