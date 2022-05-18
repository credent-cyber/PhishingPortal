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

        readonly TenantRepository _tenantRepository;
        public TenantController(ILogger<TenantController> logger, ITenantAdminRepository adminRepository, IHttpContextAccessor httpContextAccessor) :
            base(logger, adminRepository, httpContextAccessor)
        {
            _tenantRepository = new TenantRepository(logger, TenantDbCtx);
        }

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

        [HttpGet]
        [Route("recipient-by-campaign/{campaignId}")]
        public async Task<List<CampaignRecipient>> GetRecipientByCampaign(int campaignId)
        {
            var result = await _tenantRepository.GetRecipientByCampaignId(campaignId);
            return await Task.FromResult(result);
        }



        [HttpPost]
        [Route("UpsertCampaign")]
        public Campaign UpsertCampaign(Campaign campaign)
        {
            Campaign result = null;

            if (campaign == null)
                throw new ArgumentNullException("Invalid campaign data");

            if (campaign.Id > 0)
                TenantDbCtx.Campaigns.Update(campaign);
            else
                TenantDbCtx.Campaigns.Add(campaign);

            TenantDbCtx.SaveChanges();

            return campaign;
        }

        /// <summary>
        /// ImportRecipientToCampaign
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import-recipients-to-campaign/{campaignId}")]
        public async Task<ApiResponse<List<RecipientImport>>> ImportRecipientToCampaign([FromRoute] int campaignId, [FromBody] List<RecipientImport> data)
        {
            //bool hasChanges = false;
            var results = await _tenantRepository.ImportRecipientAsync(campaignId, data);

            var ispartial = data.Any(o => !string.IsNullOrEmpty(o.ValidationErrMsg));
            var response = new ApiResponse<List<RecipientImport>>
            {
                IsSuccess = true,
                Message = ispartial ? "Imported partially" : "Imported succesfully"  ,
                Result = data
            };

            return response;

        }

        [HttpGet]
        [Route("templates-by-type/{type?}")]
        public async Task<List<CampaignTemplate>> GetTemplatesByType(CampaignType? type)
        {
            var results = await _tenantRepository.GetAllTemplates(type);

            return results;
        }

        [HttpGet]
        [Route("template-by-id/{id}")]
        public async Task<CampaignTemplate> GetTemplateById(int id)
        {
            return await _tenantRepository.GetTemplateById(id);
        }


        [HttpPost]
        [Route("upsert-template")]
        public async Task<CampaignTemplate> UpsertTemplate(CampaignTemplate template)
        {
            return await _tenantRepository.UpsertTemplate(template); 
        }
    }
}
