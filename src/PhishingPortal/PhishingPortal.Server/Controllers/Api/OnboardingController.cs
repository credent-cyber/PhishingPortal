using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;

namespace PhishingPortal.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnboardingController : BaseApiController
    {

        public OnboardingController(ILogger<OnboardingController> logger, ITenantAdminRepository tenantAdminRepo) : base(logger)
        {
            tenatAdminRepo = tenantAdminRepo;
        }

        public ITenantAdminRepository tenatAdminRepo { get; }

        [HttpPost]
        public async Task<Tenant> Register(Tenant tenant)
        {
            return await tenatAdminRepo.CreateTenantAsync(tenant);
        }

    }
}
