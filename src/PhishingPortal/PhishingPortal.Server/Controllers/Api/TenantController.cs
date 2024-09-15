using Microsoft.AspNetCore.Authorization;


using PhishingPortal.Repositories;
using PhishingPortal.Dto.Dashboard;
using HtmlAgilityPack;
using PhishingPortal.Server.Services.Interfaces;
using PhishingPortal.Server.Services;
using PhishingPortal.Server.Controllers.Api.Abstraction;
using Microsoft.Graph;

namespace PhishingPortal.Server.Controllers.Api
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using PhishingPortal.Common;
    using PhishingPortal.Dto;
    using PhishingPortal.Dto.Subscription;
    using PhishingPortal.Licensing;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantController : BaseTenantController
    {

        readonly TenantRepository _tenantRepository;
        readonly string _templateImageRootPath;
        readonly IAzActDirClientService _adImportClient;
        readonly ITrainingRepository TrainingRepository;
        readonly IOnPremiseADService _onPromiseADService;

        public INsLookupHelper NsLookupHelper { get; }
        public ILicenseProvider LicenseProvider { get; }

        public TenantController(ILogger<TenantController> logger, IConfiguration appConfig, ITenantAdminRepository adminRepository,
            IHttpContextAccessor httpContextAccessor, ITenantDbResolver tenantDbResolver, INsLookupHelper nsLookupHelper, ILicenseProvider license) :
            base(logger, adminRepository, httpContextAccessor, tenantDbResolver)
        {
            _tenantRepository = new TenantRepository(logger, TenantDbCtx);

            _templateImageRootPath = appConfig.GetValue<string>("TemplateImgRootPath");

            _adImportClient = new AzActDirClientService(logger, _tenantRepository);
            TrainingRepository = new TrainingRepository(logger, TenantDbCtx);
            NsLookupHelper = nsLookupHelper;
            LicenseProvider = license;
            _onPromiseADService = new OnPremiseADService(logger, TenantDbCtx);
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
        [Route("Campaign-by-name/{Name}")]
        public async Task<Campaign> GetCampByName(string name)
        {
            return await _tenantRepository.GetCampaignByName(name);
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
                // campaign.State = CampaignStateEnum.Draft;
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
            if (template.Id > 0)
                template.ModifiedOn = DateTime.Now;
            else
                template.CreatedOn = DateTime.Now;

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
        public async Task<ApiResponse<string>> CampignLinkHit(GenericApiRequest<string> request)
        {
            var result = new ApiResponse<string>();
            try
            {
                var outcome = await _tenantRepository.CampaignHit(request.Param);
                result.IsSuccess = outcome.Item1;
                result.Message = "Successful";
                result.Result = outcome.Item2;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while hitting campaign url");
            }

            return result;

        }

        [HttpGet]
        [Route("get-latest-statistics")]
        public async Task<ApiResponse<ConsolidatedPhishingStats>> GetLatestStatistics()
        {
            var result = new ApiResponse<ConsolidatedPhishingStats>();

            try
            {
                var data = await _tenantRepository.GetLastPhishingStatics();

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
                result.Message = string.Empty;
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


        #region Azure Ad Integration

        [HttpGet]
        [Route("az-ad-groups")]

        public async Task<Dictionary<string, string>?> GetAzureAdUserGroups()
        {
            var data = _adImportClient.GetAllUserGroups();
            if (data.Result is null)
                return null;
            else
                return await _adImportClient.GetAllUserGroups();
        }

        [HttpGet]
        [Route("recipient-user-groups")]

        public async Task<List<RecipientGroup>> GetRecipientGroups(bool adGroupsOnly = false)
        {
            var result = await _tenantRepository.GetRecipientGroups(adGroupsOnly);

            return result;
        }

        [HttpPost]
        [Route("az-ad-user-groups-import")]

        public async Task<ApiResponse<List<Recipient>>> ImportAdUserGroups(RecipientGroup adRecipientGroup)
        {
            var response = new ApiResponse<List<Recipient>>();

            var adUsers = new List<Microsoft.Graph.User>();

            if (adRecipientGroup.Uid == "all-users")
            {
                adUsers = await _adImportClient.GetAdUsers();
            }
            else
            {
                adUsers = await _adImportClient.GetGroupMembers(adRecipientGroup.Uid);
            }

            if (adUsers != null && adUsers.Count() > 0)
            {
                var recipients = adUsers.Select(o => new Recipient
                {
                    IsActive = true,
                    IsADUser = true,
                    EmployeeCode = o.EmployeeId,
                    Name = o.DisplayName,
                    Mobile = o.MobilePhone,
                    Address = o.OfficeLocation,
                    DateOfBirth = o.Birthday?.ToString("dd MMM"),
                    Department = o.Department,
                    Branch = o.OfficeLocation,
                    Email = o.Mail,
                    WhatsAppNo = o.MobilePhone
                }).ToList();

                var result = await _tenantRepository.ImportAdGroupMembers(adRecipientGroup, recipients);


                response.IsSuccess = true;
                response.Result = result;
            }

            return response;
        }

        #endregion

        [HttpGet]
        [Route("settings")]

        public async Task<Dictionary<string, string>> GetSettings()
        {
            var result = await _tenantRepository.GetSettings();

            return result;
        }

        [HttpGet]
        [Route("subscription")]

        public async Task<ApiResponse<SubscriptionInfo>> GetSubscriptionKey()
        {
            // returns current subscription.
            var result = _tenantRepository.GetLicenseKeys();
            var license = result.LicenseKey;
            var publicKey = result.PublicKey;

            if (license == null || string.IsNullOrEmpty(license)
                || publicKey == null || string.IsNullOrEmpty(publicKey))
            {
                return await Task.FromResult(new ApiResponse<SubscriptionInfo>() { IsSuccess = false });
            }

            var subscriptionInfo = LicenseProvider.GetSubscriptionInfo(license, publicKey);

            return await Task.FromResult(new ApiResponse<SubscriptionInfo>()
            {
                IsSuccess = subscriptionInfo != null,
                Result = subscriptionInfo
            });
        }

        [HttpPost]
        [Route("upsert-settings")]

        public async Task<Dictionary<string, string>> UpsertSettings(Dictionary<string, string> settings)
        {
            var result = await _tenantRepository.UpsertSettings(settings, HttpContextAccessor?.HttpContext?.GetCurrentUser());

            return result;
        }


        [HttpPost]
        [Route("upsert-training")]
        public async Task<Training> UpsertTraining(Training training)
        {
            if (training.Id > 0)
                training.ModifiedOn = DateTime.Now;
            else
                training.CreatedOn = DateTime.Now;

            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(training.Content);

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

            training.Content = htmlDoc.DocumentNode.WriteTo();

            return await _tenantRepository.UpsertTraining(training);
        }

        [HttpGet]
        [Route("training-by-id/{id}")]
        public async Task<Training> GetTrainingById(int id)
        {
            return await _tenantRepository.GetTrainingById(id);
        }


        [HttpPost]
        [Route("import-recipients-to-training/{trainingId}")]
        public async Task<ApiResponse<List<RecipientImport>>> ImportRecipientToTraining([FromRoute] int trainingId, [FromBody] List<RecipientImport> data)
        {
            //bool hasChanges = false;
            var results = await _tenantRepository.ImportTrainingRecipient(trainingId, data);

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
        [Route("recipient-by-training/{trainingId}")]
        public async Task<List<TrainingRecipients>> GetRecipientByTraining(int trainingId)
        {
            var result = await _tenantRepository.GetRecipientByTrainingId(trainingId);
            return await Task.FromResult(result);
        }


        [HttpPost]
        [Route("training")]
        [AllowAnonymous]
        public async Task<ApiResponse<string>> TrainingLink(GenericApiRequest<string> request)
        {
            var result = new ApiResponse<string>();
            try
            {
                var outcome = await _tenantRepository.Training(request.Param);
                result.IsSuccess = outcome.Item1;
                result.Message = "Successful";
                result.Result = outcome.Item2;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while hitting training url");
                result.IsSuccess = false;
                result.Message = "Error occurred while processing the request";
            }

            return result;
        }

        [HttpGet]
        [Route("monthwise-training-data/{year}")]
        public async Task<ApiResponse<MonthlyTrainingBarChart>> GetMonthwiseTrainingData(int year)
        {
            var result = new ApiResponse<MonthlyTrainingBarChart>();
            try
            {
                var data = await _tenantRepository.GetTrainingReportData(year);
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

        [HttpGet]
        [Route("get-training-statistics")]
        public async Task<ApiResponse<TrainingStatics>> GetTrainingStatistics()
        {
            var result = new ApiResponse<TrainingStatics>();

            try
            {
                var data = await _tenantRepository.GetLastTrainingStatics();

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

        [HttpGet]
        [Route("GetYearsfromCampaignlog")]
        public async Task<List<int>> GetYearListfromCampaignLog()
        {
            var result = await _tenantRepository.GetYearList();
            return result.ToList();
        }

        #region MyTraining

        [HttpGet]
        [Route("GetMyTrainings")]
        public async Task<List<MyTraining>> GetMyTrainings()
        {
            var output = new List<MyTraining>();

            var email = HttpContextAccessor?.HttpContext?.GetUserEmail();

            if (string.IsNullOrEmpty(email))
                Forbid("Unauthorized user");

            var result = await TrainingRepository.GetMyTrainings(email);

            foreach (var r in result)
            {
                output.Add(new MyTraining()
                {
                    Training = r.Training,
                    TrainingLog = r.TrainingLog
                });
            }

            return output;
        }

        [HttpGet]
        [Route("GetTrainingByUniqueId")]
        public async Task<(Training Training, TrainingLog TrainingLog)> GetTrainingByUniqueId(string uniqueID)
        {
            var email = HttpContextAccessor?.HttpContext?.GetUserEmail();

            if (string.IsNullOrEmpty(email))
                Forbid("Unauthorized user");

            return await TrainingRepository.GetTrainingByUniqueID(new Guid(uniqueID), email);
        }

        [HttpPost]
        [Route("UpdateTrainingProgress")]
        public async Task<TrainingLog> UpdateTrainingProgress([FromBody] TrainingProgress progress)
        {
            var email = HttpContextAccessor?.HttpContext?.GetUserEmail();

            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("User not authorized");

            return await TrainingRepository.UpdateTrainingProgress(new Guid(progress.UniqueID), progress.Value, email, progress.CheckPoint);
        }
        #endregion

        [HttpGet]
        [Route("GetCampaignsNames")]
        public async Task<List<Campaign>> GetCompaignNames()
        {
            try
            {
                var result = await _tenantRepository.GetCampaignsName();
                return result.ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
        [HttpPost]
        [Route("TrainingCampIdMap")]
        public async Task<ApiResponse<string>> TrainingCampIdMapping(Dictionary<int, List<int>> dict)
        {
            var result = new ApiResponse<string>();
            try
            {
                var outcome = await _tenantRepository.UpsertTrainingCampaignMap(dict);
                result.Message = "Successful";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while uploading data to TrainingCampIdMapping");
            }

            return result;
        }

        [HttpGet]
        [Route("GetTrainingByCampaignId/{CampId}")]
        public async Task<TrainingCampaignMapping> GetTrainingByCampaignId(int CampId)
        {
            var data =  await _tenantRepository.GetTrainingByCampaignId(CampId);
            return data;
        }

        [HttpPost]
        [Route("UpsertCampaignTrainingMap")]
        public async Task<ApiResponse<string>> UpsertCampaignTrainingMap(CampaignTrainingIdcs campaignTrainingIdcs)
        {
            var result = new ApiResponse<string>();
            try
            {
                var outcome = await _tenantRepository.UpsertCampaignTrainingMap(campaignTrainingIdcs);
                result.Message = "Successful";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while uploading data to TrainingCampIdMapping");
            }

            return result;
        }

        [HttpGet]
        [Route("getTrainingCampaignIds/{trainingId}")]
        public async Task<List<TrainingCampaignMapping>> GetTrainingCampaignsId(int trainingId)
        {
            var result = await _tenantRepository.GetTrainingCampaignsId(trainingId);
            return await Task.FromResult(result);
        }


        [HttpGet]
        [Route("GetTrainingVideo")]
        public async Task<IEnumerable<TrainingVideo>> GetTrainingVideo()
        {
            try
            {
                var result = await _tenantRepository.GetTrainingVideos();
                return result.ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
        [HttpPost]
        [Route("upsert-trainingVideo")]
        public async Task<TrainingVideo> UpsertTrainingVideoDetail(TrainingVideo trainingVideo)
        {

            return await _tenantRepository.UpsertTrainingVideo(trainingVideo);
        }

        [HttpGet]
        [Route("GetTrainings")]
        public async Task<List<Training>> GetAllTrainings()
        {
            try
            {
                var result = await _tenantRepository.GetAllTraining();
                return result.ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("UpsertTrainingQuiz")]
        public async Task<List<TrainingQuizQuestion>> UpsertTrainingQuiz(List<TrainingQuizQuestion> trainingQuiz)
        {

            return await _tenantRepository.UpsertTrainingQuiz(trainingQuiz);
        }

        [HttpGet]
        [Route("TrainingQuiz-by-id/{id}")]
        public async Task<TrainingQuizResult> GetTrainingQuizById(int id)
        {
            return await _tenantRepository.GetTrainingQuizById(id);
        }

        [HttpGet]
        [Route("training-quiz-by-training-id/{trainingId}")]
        public async Task<IEnumerable<TrainingQuizQuestion>> GetTrainingQuizByTrainingId(int trainingId)
        {
            return await _tenantRepository.GetQuizByTrainingId(trainingId);
        }

        [HttpPost]
        [Route("UpsertTrainingQuizTitle")]
        public async Task<List<TrainingQuiz>> UpsertTrainingQuizTitle(List<TrainingQuiz> Quiz)
        {
            return await _tenantRepository.UpsertTrainingQuizTitle(Quiz);
        }

        [HttpGet]
        [Route("AllTrainingQuiz")]
        public async Task<IEnumerable<TrainingQuiz>> GetAllTrainingQuiz()
        {
            return await _tenantRepository.GetAllTrainingQuiz();
        }

        [HttpPost]
        [Route("DeleteTrainingQuizQuestion/{id}")]
        public async Task<ApiResponse<TrainingQuizQuestion>> DeleteTrainingQuizQuestion(int id)
        {
            return await _tenantRepository.DeleteTrainingQuizQuestion(id);
        }


        [HttpGet]
        [Route("GetAllDomains")]
        public async Task<IEnumerable<TenantDomain>> GetAllDomains()
        {
            return await AdminRepository.GetDomains(Tenant.Id);
        }

        [HttpPost]
        [Route("UpsertMyDomain")]
        public async Task<ApiResponse<TenantDomain>> UpsertDomain(TenantDomain tenantDomain)
        {
            try
            {
                if (tenantDomain.Id == 0)
                {
                    tenantDomain.TenantId = Tenant.Id;
                    tenantDomain.DomainVerificationCode = Tenant.UniqueId;
                    tenantDomain.CreatedBy = CurrentUser;
                    tenantDomain.CreatedOn = DateTime.Now;
                }

                var result = await AdminRepository.UpsertTenantDomain(tenantDomain);

                return new ApiResponse<TenantDomain> { Result = result, IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TenantDomain> { Message = ex.Message };
            }
        }

        [HttpPost]
        [Route("VerifyDomain")]
        public async Task<TenantDomain> VerifyDomain(TenantDomain entry)
        {

#if DEBUG
            var verficationResult = true;
#else
            var verficationResult = NsLookupHelper.VerifyDnsRecords("TXT", entry.Domain.Trim().ToLower(), entry.DomainVerificationCode);
#endif
            if (verficationResult)
            {
                entry.ModifiedBy = CurrentUser;
                entry.ModifiedOn = DateTime.Now;
                return await AdminRepository.VerifyDomain(entry);
            }

            return entry;
        }

        [HttpPost]
        [Route("DeleteDomain/{id}")]
        public async Task<ApiResponse<bool>> DeleteDomain(int id)
        {
            try
            {
                var result = await AdminRepository.DeleteDomain(id);

                return new ApiResponse<bool> { Result = result, IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Message = ex.Message };
            }
        }

        #region OnPromiseAD
        [HttpGet]
        [Route("GetOnPremiseADGroups")]
        public async Task<Dictionary<string, List<OnPremiseADUsers>>> GetOnPremiseADGroups()
        {
          return await _onPromiseADService.GetOnPremiseADGroups();
        }

        [HttpPost]
        [Route("GetOnPremiseUsersByADGroup")]
        public async Task<List<OnPremiseADUsers>> GetOnPremiseUsersByADGroup([FromBody] string groupName)
        {
            return await _onPromiseADService.GetOnPremiseUsersByADGroup(groupName);
        }
        #endregion

        [HttpPost]
        [Route("campaign-spam-report")]
        [AllowAnonymous]
        public async Task<ApiResponse<string>> CampignLinkReport(GenericApiRequest<string> request)
        {

            return await _tenantRepository.CampaignSpamReport(request.Param);

        }

        #region Report
        [HttpGet]
        [Route("BarChartDrillDownReportCount/{campaignId}")]
        public async Task<IEnumerable<CampaignLog>> BarChartDrillDownReportCount(int campaignId)
        {
            return await _tenantRepository.BarChartDrillDownReportCount(campaignId);
        }


        [HttpPost]
        [Route("PieChartDrillDownReportCount")]
        public async Task<IQueryable<ReportDataCounts>> PieChartDrillDownReportCount([FromBody] DrillDownReportCountParameter parameters)
        {
                return await _tenantRepository.PieChartDrillDownReportCount(parameters);        
        }
        #endregion
    }
}
