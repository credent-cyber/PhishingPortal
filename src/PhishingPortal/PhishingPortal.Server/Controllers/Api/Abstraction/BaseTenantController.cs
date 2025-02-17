﻿using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Auth;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Controllers.Api.Abstraction
{
    public class BaseTenantController : BaseApiController
    {
        public TenantDbContext TenantDbCtx { get; private set; }
        public Tenant Tenant { get; private set; }
        public ITenantAdminRepository AdminRepository { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public ITenantDbResolver TenantDbResolver { get; }
        public string CurrentUser => HttpContextAccessor?.HttpContext?.GetCurrentUser(); 

        public BaseTenantController(ILogger logger, ITenantAdminRepository adminRepository, IHttpContextAccessor httpContextAccessor,
            ITenantDbResolver tenantDbResolver)
            : base(logger)
        {
            AdminRepository = adminRepository;
            HttpContextAccessor = httpContextAccessor;
            TenantDbResolver = tenantDbResolver;
            Tenant = tenantDbResolver.Tenant;
            TenantDbCtx = TenantDbResolver.TenantDbCtx;

        }

    }
}
