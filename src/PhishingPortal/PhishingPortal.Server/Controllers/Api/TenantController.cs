﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto.Dashboard;
using PhishingPortal.Server.Services;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace PhishingPortal.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantController : BaseTenantController
    {

        readonly TenantRepository _tenantRepository;
        readonly string _templateImageRootPath;
      
        public TenantController(ILogger<TenantController> logger, IConfiguration appConfig, ITenantAdminRepository adminRepository, 
            IHttpContextAccessor httpContextAccessor, ITenantDbResolver tenantDbResolver) :
            base(logger, adminRepository, httpContextAccessor, tenantDbResolver)
        {
            _tenantRepository = new TenantRepository(logger, TenantDbCtx);

            _templateImageRootPath = appConfig.GetValue<string>("TemplateImgRootPath");
        }

        [HttpGet]
        [Route("Campaigns")]
        [Route("Campaigns/{pageIndex?}/{pageSize?}")]
        public async Task<IEnumerable<Campaign>> Get(int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                var result = await _tenantRepository.GetAllCampaigns(pageIndex, pageSize);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
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
            {
                campaign.CreatedOn = DateTime.Now;
                campaign.State = CampaignStateEnum.Draft;
            }
                
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
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(template.Content);

            foreach (var childNode in htmlDoc.DocumentNode.Descendants(2))
            {
                try
                {
                    if (childNode.Name != "img")
                        continue;

                    var src = childNode.Attributes["src"].Value;
                    var alt = childNode.Attributes["alt"]?.Value;
                   
                    if (!src.StartsWith("data:image"))
                        continue;
                    var type = src.Split(";")[0];
                    
                    var encoded_value = src.Split(";")[1];
                    var code = encoded_value.Split(",")[0];
                    var base64Content = encoded_value.Split(",")[1];
                    var bytes = Convert.FromBase64String(base64Content);
                    var ext = "jpeg";
                    var nm = $"{Guid.NewGuid().ToString()}.{ext}";
                    System.IO.File.WriteAllBytes(Path.Combine(_templateImageRootPath, nm), bytes);

                    childNode.SetAttributeValue("src", $"img/email/{nm}");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            template.Content = htmlDoc.DocumentNode.WriteTo();

            return await _tenantRepository.UpsertTemplate(template);
        }

        [HttpPost]
        [Route("campaign-hit")]
        [AllowAnonymous]
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
        [Route("monthly-phishing-bar-chart-data/{year}")]
        public async Task<ApiResponse<MonthlyPhishingBarChart>> GetMonthlyBarChartEntires(int year)
        {
            var result = new ApiResponse<MonthlyPhishingBarChart>();

            try
            {
                var data = await _tenantRepository.GetMonthlyBarChart(year);

                result.IsSuccess = true;
                result.Message = "Success";
                result.Result = data;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
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
        public async Task<ApiResponse<CategoryWisePhishingTestData>> GetCategoryWisePhishingTestPronePercentage(DateTime startDate, DateTime endDate)
        {
            var result = new ApiResponse<CategoryWisePhishingTestData>();
            try
            {
                var data = await _tenantRepository.GetCategoryWisePhishingReport(startDate, endDate);
                result.Result = data;
                result.IsSuccess = true;
                result.Message = String.Empty;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

    }
}
