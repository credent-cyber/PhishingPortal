using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PhishingPortal.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantController : BaseTenantController
    {
        public TenantController(ILogger<TenantController> logger, ITenantAdminRepository adminRepository, IHttpContextAccessor httpContextAccessor) : 
            base(logger, adminRepository, httpContextAccessor)
        { }

        [HttpGet]
        [Route("Campaigns")]
        [Route("Campaigns/{pageIndex?}/{pageSize?}")]
        public IEnumerable<Campaign> Get(int pageIndex = 0, int pageSize = 10)
        {
            var result = Enumerable.Empty<Campaign>();

            result = TenantDbCtx.Campaigns.Include(o => o.Schedule)
                .Skip(pageIndex * pageSize).Take(pageSize); 
            
            return result;
        }

    }
}
