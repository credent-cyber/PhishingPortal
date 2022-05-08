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
        [Route("Register")]
        public async Task<Tenant> Register(Tenant tenant)
        {
            return await tenatAdminRepo.CreateTenantAsync(tenant);
        }

        [HttpGet]
        public async Task<List<Tenant>> GetAll(int pageIndex, int pageSize)
        {
            return await tenatAdminRepo.GetAllAsync(pageIndex, pageSize);
        }

        [HttpGet]
        [Route("TenantByUniqueId")]
        public async Task<Tenant> GetByUniqueId(string uniqueId)
        {
            return await tenatAdminRepo.GetByUniqueId(uniqueId);
        }

        [HttpPost]
        [Route("Provision")]
        public async Task Provision(ProvisionTenantRequest request)
        {
            var result = await tenatAdminRepo.ProvisionAsync(request.TenantId, request.ConnectionString);

            if (result == false)
                NotFound("Tenant not registered yet");
        }

    }
}
