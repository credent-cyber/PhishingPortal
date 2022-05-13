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

        [HttpGet]
        [Route("Campaign/{id}")]
        public Campaign Get(int id)
        {
            Campaign result = null;

            result = TenantDbCtx.Campaigns.Include(o => o.Schedule).Include(o => o.Detail)
                .FirstOrDefault(o => o.Id == id);

            return result;
        }


        [HttpGet]
        [Route("CampaignTemplates")]
        public IEnumerable<CampaignTemplate> Get()
        {
            IEnumerable<CampaignTemplate> result = null;

            result = TenantDbCtx.CampaignTemplates.ToList();

            return result;
        }


        [HttpPost]
        [Route("UpsertCampaign")]
        public Campaign UpsertCampaign(Campaign campaign)
        {
            Campaign result = null;

            if (campaign == null)
                throw new ArgumentNullException("Invalid campaign data");

            if(campaign.Id > 0)
                TenantDbCtx.Campaigns.Update(campaign);
            else
                TenantDbCtx.Campaigns.Add(campaign);
            
            TenantDbCtx.SaveChanges();

            return campaign;
        }
    }
}
