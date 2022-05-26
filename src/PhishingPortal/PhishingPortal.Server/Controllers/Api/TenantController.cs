using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto.Dashboard;

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
        public async Task<IEnumerable<Campaign>> Get(int pageIndex = 0, int pageSize = 10)
        {
            return await _tenantRepository.GetAllCampaigns(pageIndex, pageSize);
        }

        [HttpGet]
        [Route("Campaign/{id}")]
        public async Task<Campaign> Get(int id)
        {
            return await _tenantRepository.GetCampaignById(id);
        }


        [HttpGet]
        [Route("CampaignTemplates/{pageIndex?}/{pageSize?}")]
        public async Task<IEnumerable<CampaignTemplate>> GetAllTemplates(int pageIndex = 0, int pageSize = 10)
        {
            return await _tenantRepository.GetAllTemplates(pageIndex, pageSize);
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
        public async Task<Campaign> UpsertCampaign(Campaign campaign)
        {
            if (campaign.Id > 0)
                campaign.ModifiedOn = DateTime.Now;
            else
                campaign.CreatedOn = DateTime.Now;
            campaign.State = CampaignStateEnum.Draft;
            return await _tenantRepository.UpsertCampaign(campaign);
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
                Message = ispartial ? "Imported partially" : "Imported succesfully",
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

        [HttpPost]
        [Route("campaign-hit")]
        public async Task<ApiResponse<bool>> CampignLinkHit(GenericApiRequest<string> request)
        {
            var result = new ApiResponse<bool>();
            try
            {
                bool outcome = await _tenantRepository.CampaignHit(request.Param);
                result.IsSuccess = outcome;
                result.Message = "Successful";
                result.Result = outcome;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while hitting campaign url");
            }

            return result;

        }


        /// <summary>
        /// Monthly phishing chart for the year input
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
       [HttpGet]
        [Route("monthly-phishing-bar-chart-data/{year?}")]
        public async Task<ApiResponse<MonthlyPhishingBarChartEntry>> GetMonthlyBarChartEntires(int year)
        {
            var result = new ApiResponse<MonthlyPhishingBarChartEntry>();

            throw new NotImplementedException();
        }

         
        /// <summary>
        /// Provides data for catewory wise phishing test prone percentage
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("category-wise-phising-test-prone-percent/{startDate}/{endDate}")]
        public async Task<ApiResponse<CategoryWisePhishingTestDData>> GetCategoryWisePhishingTestPronePercentage(DateTime startDate, DateTime endDate)
        {
            var result = new ApiResponse<CategoryWisePhishingTestDData>();

            throw new NotImplementedException();
        }

    }
}
