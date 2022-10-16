using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Controllers.Api.Abstraction
{
    public class BaseTenantController : BaseApiController
    {
        public TenantDbContext TenantDbCtx { get; private set; }
        public Tenant Tenant { get; private set; }
        public ITenantDbResolver TenantDbResolver { get; }

        public BaseTenantController(ILogger logger, ITenantAdminRepository adminRepository, IHttpContextAccessor httpContextAccessor,
            ITenantDbResolver tenantDbResolver)
            : base(logger)
        {
            TenantDbResolver = tenantDbResolver;
            Tenant = tenantDbResolver.Tenant;
            TenantDbCtx = TenantDbResolver.TenantDbCtx;

        }

    }
}
