﻿using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Dashboard;
using PhishingPortal.Dto.Subscription;
using PhishingPortal.UI.Blazor.Pages;
using System;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Client
{
    public class TenantClient : BaseHttpClient
    {
        public TenantClient(ILogger<TenantClient> logger, HttpClient httpClient)
            : base(logger, httpClient)
        {

        }
        [Inject]
        CustomStateProvider StateProvider { get; }

        public async Task<IEnumerable<Campaign>> GetCampaignsAsync(int pageIndex, int pageSize)
        {
            IEnumerable<Campaign> campaigns;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/Campaigns/{pageIndex}/{pageSize}");

                res.EnsureSuccessStatusCode();

                campaigns = await res.Content.ReadFromJsonAsync<IEnumerable<Campaign>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return campaigns;
        }

        public async Task<Campaign> GetCampaingById(int id)
        {
            Campaign campaign = null;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/Campaign/{id}");

                res.EnsureSuccessStatusCode();

                campaign = await res.Content.ReadFromJsonAsync<Campaign>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return campaign;
        }
        public async Task<Campaign> GetCampaingByName(string name)
        {
            Campaign campaign = null;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/Campaign-by-name/{name}");

                res.EnsureSuccessStatusCode();

                campaign = await res.Content.ReadFromJsonAsync<Campaign>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return campaign;
        }
        public async Task<IEnumerable<CampaignTemplate>> GetCampaignTemplates()
        {
            List<CampaignTemplate> templates = Enumerable.Empty<CampaignTemplate>().ToList();

            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/CampaignTemplates");

                res.EnsureSuccessStatusCode();

                templates = await res.Content.ReadFromJsonAsync<List<CampaignTemplate>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return templates;
        }

        public async Task<Campaign> UpsertCampaignAsync(Campaign campaign)
        {

            Campaign result = null;

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/Tenant/UpsertCampaign", campaign);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<Campaign>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;

        }

        /// <summary>
        /// Imports a list of recipients (uniquely identified by email id)
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public async Task<List<RecipientImport>> ImportRecipientToCampaign(int campaignId, List<RecipientImport> recipients)
        {
            ApiResponse<List<RecipientImport>> result;
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/import-recipients-to-campaign/{campaignId}", recipients);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<ApiResponse<List<RecipientImport>>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return await Task.FromResult(result.Result);
        }

        public async Task<List<CampaignRecipient>> GetRecipientByCampaignId(int campaignId)
        {
            List<CampaignRecipient> result = new List<CampaignRecipient>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/recipient-by-campaign/{campaignId}");

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<CampaignRecipient>>();



            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return await Task.FromResult(result);
        }

        public async Task<List<CampaignTemplate>> GetTemplatesByType(CampaignType? type)
        {
            List<CampaignTemplate> result;

            try
            {
                var url = $"api/tenant/templates-by-type";
                if (type.HasValue)
                    url += $"/{type.Value}";
                var res = await HttpClient.GetAsync(url);
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<CampaignTemplate>>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<CampaignTemplate> GetTemplateById(int id)
        {
            CampaignTemplate result;

            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/template-by-id/{id}");
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<CampaignTemplate>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }


        public async Task<CampaignTemplate> UpsertCampaignTemplate(CampaignTemplate template)
        {
            CampaignTemplate result;

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/upsert-template", template);
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<CampaignTemplate>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<bool> CampaignHit(string urlKey)
        {
            try
            {
                var request = new GenericApiRequest<string>() { Param = urlKey };
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/campaign-hit", request);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return false;
        }

        public async Task<MonthlyPhishingBarChart?> GetMonthlyBarChartEntires(int year)
        {
            try
            {
                var result = new MonthlyPhishingBarChart();
                var res = await HttpClient.GetAsync($"api/tenant/monthly-phishing-bar-chart-data/{year}");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return result;

                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<MonthlyPhishingBarChart>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return null;
        }

        public async Task<ConsolidatedPhishingStats> GetLatestStats()
        {
            // get-latest-statistics
            var result = new ConsolidatedPhishingStats();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/get-latest-statistics");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return result;
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<ConsolidatedPhishingStats>>();

                if (json != null)
                    result = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }
        public async Task<CategoryWisePhishingTestData?> GetCategoryWisePhishingTestData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/category-wise-phising-test-prone-percent/{startDate.ToString("MM-dd-yyyy")}/{endDate.ToString("MM-dd-yyyy")}");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return null;
                res.EnsureSuccessStatusCode();


                var json = await res.Content.ReadFromJsonAsync<ApiResponse<CategoryWisePhishingTestData>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return null;
        }

        public async Task<SubscriptionInfo> GetSubscription()
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/subscription");
               
                res.EnsureSuccessStatusCode();

                var content = await res.Content.ReadFromJsonAsync<ApiResponse<SubscriptionInfo>>();
                
                return content.Result;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }

            return null;
        }

        #region Settings
        public async Task<Dictionary<string, string>> GetSettings()
        {
            var response = new Dictionary<string, string>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/settings");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (json != null)
                    response = json;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }

        public async Task<Dictionary<string, string>> UpsertSettings(Dictionary<string, string> settings)
        {
            var response = new Dictionary<string, string>();
            try
            {
                var res = await HttpClient.PostAsJsonAsync<Dictionary<string, string>>($"api/tenant/upsert-settings", settings);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (json != null)
                    response = json;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }
        #endregion

        #region Related to Azure Active Directory Integration

        /// <summary>
        /// Get all Azure AD User Groups if available
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>?> GetAzureADUserGroups()
        {
            var response = new Dictionary<string, string>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/az-ad-groups");
                res.EnsureSuccessStatusCode();
                if (res.StatusCode.ToString().Equals("NoContent"))
                    return null;

                var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (json != null)
                    response = json;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }

        /// <summary>
        /// Get list of recipient groups Azure AD + non Ad groups
        /// </summary>
        /// <returns></returns>
        public async Task<List<RecipientGroup>> GetRecipientUserGroups()
        {
            var response = new List<RecipientGroup>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/recipient-user-groups");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<List<RecipientGroup>>>();
                if (json != null)
                    response = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }

        /// <summary>
        /// Mark Azure AD User groups to be imported
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public async Task<List<Recipient>> ImportAzureADByUserGroups(RecipientGroup grp)
        {
            var response = new List<Recipient>();

            try
            {
                var res = await HttpClient.PostAsJsonAsync<RecipientGroup>($"api/tenant/az-ad-user-groups-import", grp);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<List<Recipient>>>();
                if (json != null)
                    response = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }


        #endregion

        public async Task<Training> UpsertTraining(Training training)
        {
            Training result;

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/upsert-training", training);
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<Training>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<Training> GetTrainingById(int id)
        {
            Training result;

            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/training-by-id/{id}");
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<Training>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<List<RecipientImport>> ImportRecipientToTraining(int trainingId, List<RecipientImport> recipients)
        {
            ApiResponse<List<RecipientImport>> result;
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/import-recipients-to-training/{trainingId}", recipients);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<ApiResponse<List<RecipientImport>>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return await Task.FromResult(result.Result);
        }

        public async Task<List<TrainingRecipients>> GetRecipientByTrainingId(int trainingId)
        {
            List<TrainingRecipients> result = new List<TrainingRecipients>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/recipient-by-training/{trainingId}");

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<TrainingRecipients>>();



            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return await Task.FromResult(result);
        }


        public async Task<MonthlyTrainingBarChart?> GetMonthwiseTrainingData(int year)
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/monthwise-training-data/{year}");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return null;
                res.EnsureSuccessStatusCode();


                var json = await res.Content.ReadFromJsonAsync<ApiResponse<MonthlyTrainingBarChart>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return null;
        }

        public async Task<TrainingStatics> GetLatestTrainingStats()
        {
            var result = new TrainingStatics();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/get-training-statistics");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return result;
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<TrainingStatics>>();

                if (json != null)
                    result = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<List<int>> GetCampaignLogYears()
        {
            List<int> years = new List<int>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/GetYearsfromCampaignlog");
                res.EnsureSuccessStatusCode();
                years = await res.Content.ReadFromJsonAsync<List<int>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }
            return years;
        }

        public async Task<List<Campaign>> GetCampaignsNames()
        {
            List<Campaign> campaigns;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/GetCampaignsNames");

                res.EnsureSuccessStatusCode();

                campaigns = await res.Content.ReadFromJsonAsync<List<Campaign>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return campaigns;
        }

        public async Task<bool> UpsertTRainingCampIDsMap(Dictionary<int, List<int>> dict)
        {
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/TrainingCampIdMap", dict);
                res.EnsureSuccessStatusCode();

                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

        }
        public async Task<bool> UpsertCampaignTrainingMap(CampaignTrainingIdcs campaignTrainingIdcs)
        {
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/UpsertCampaignTrainingMap", campaignTrainingIdcs);
                res.EnsureSuccessStatusCode();

                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

        }



        public async Task<TrainingCampaignMapping> GetTrainingByCampaignId(int id)
        {
            TrainingCampaignMapping trainingCampaign = null;
            try
            {
                var response = await HttpClient.GetAsync($"api/Tenant/GetTrainingByCampaignId/{id}");
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseContent))
                    return null;
                trainingCampaign = await response.Content.ReadFromJsonAsync<TrainingCampaignMapping>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return trainingCampaign;
        }

        public async Task<List<TrainingCampaignMapping>> GetTrainingCampaignIDs(int id)
        {
            List<TrainingCampaignMapping> Trainingcampaign = null;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/getTrainingCampaignIds/{id}");

                res.EnsureSuccessStatusCode();

                Trainingcampaign = await res.Content.ReadFromJsonAsync<List<TrainingCampaignMapping>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }
            return Trainingcampaign;
        }

        public async Task<IEnumerable<TrainingVideo>> GetTrainigVideo()
        {
            IEnumerable<TrainingVideo> trainigVideo;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/GetTrainingVideo");

                res.EnsureSuccessStatusCode();

                trainigVideo = await res.Content.ReadFromJsonAsync<IEnumerable<TrainingVideo>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return trainigVideo;
        }
        public async Task<TrainingVideo> UpsertTrainingVideo(TrainingVideo trainingVideo)
        {
            TrainingVideo result;

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/upsert-trainingVideo", trainingVideo);
                res.EnsureSuccessStatusCode();
                result = await res.Content.ReadFromJsonAsync<TrainingVideo>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<List<Training>> GetAllTrainings()
        {
            List<Training> training;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/GetTrainings");

                res.EnsureSuccessStatusCode();

                training = await res.Content.ReadFromJsonAsync<List<Training>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return training;
        }
        public async Task<List<TrainingQuizQuestion>> UpsertTrainingQuizAsync(List<TrainingQuizQuestion> trainingQuiz)
        {
            List<TrainingQuizQuestion> result = null;
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/Tenant/UpsertTrainingQuiz", trainingQuiz);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<TrainingQuizQuestion>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }
            return result;

        }

        public async Task<TrainingQuizResult> GetTrainingQuizById(int id)
        {
            TrainingQuizResult result;

            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/trainingquiz-by-id/{id}");
                res.EnsureSuccessStatusCode();
                result = await res.Content.ReadFromJsonAsync<TrainingQuizResult>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<List<MyTraining>> GetMyTrainings()
        {
            var myTrainings = new List<MyTraining>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/GetMyTrainings");
                res.EnsureSuccessStatusCode();
                myTrainings = await res.Content.ReadFromJsonAsync<List<MyTraining>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }
            return myTrainings;
        }

        public async Task<(Training Training, TrainingLog TrainingLog)> GetTrainingDetails(string uniqueID)
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/GetTrainingByUniqueId?uniqueID={uniqueID}");
                res.EnsureSuccessStatusCode();
                var str = await res.Content.ReadAsStringAsync();

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<(Training, TrainingLog)>(str);
                return (result.Item1, result.Item2);

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);

            }

            return default((Training, TrainingLog));
        }

        public async Task<TrainingLog> UpdateTrainingProgress(string uniqueID, decimal percentage, string checkpoint)
        {
            try
            {
                var res = await HttpClient.PostAsJsonAsync<TrainingProgress>($"api/tenant/UpdateTrainingProgress", new TrainingProgress
                {
                    UniqueID = uniqueID,
                    CheckPoint = checkpoint,
                    Value = percentage
                });

                res.EnsureSuccessStatusCode();

                return await res.Content.ReadFromJsonAsync<TrainingLog>();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TrainingQuiz>> UpsertTrainingQuizTitle(List<TrainingQuiz> data)
        {
            List<TrainingQuiz> result = null;
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/UpsertTrainingQuizTitle", data);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<TrainingQuiz>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }
            return result;
        }

        public async Task<IEnumerable<TrainingQuiz>> GetAllTrainingQuizTitles()
        {
            IEnumerable<TrainingQuiz> details = null;
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/AllTrainingQuiz");
                res.EnsureSuccessStatusCode();
                details = await res.Content.ReadFromJsonAsync<IEnumerable<TrainingQuiz>>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }
            return details;
        }

        public async Task<IEnumerable<TenantDomain>> GetDomains()
        {
            IEnumerable<TenantDomain> details = null;
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/GetAllDomains");
                res.EnsureSuccessStatusCode();
                details = await res.Content.ReadFromJsonAsync<IEnumerable<TenantDomain>>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }
            return details;
        }

        public async Task<(bool Status, TenantDomain data, string Message)> UpsertDomain(TenantDomain domain)
        {
            var res = await HttpClient.PostAsJsonAsync($"api/tenant/UpsertMyDomain", domain);

            res.EnsureSuccessStatusCode();

            var data = await res.Content.ReadFromJsonAsync<ApiResponse<TenantDomain>>();

            return (data.IsSuccess, data.Result, data.Message);
        }

        public async Task<TenantDomain> VerifyDomain(TenantDomain domain)
        {
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/VerifyDomain", domain);

                res.EnsureSuccessStatusCode();

                return await res.Content.ReadFromJsonAsync<TenantDomain>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }

            return default;
        }

        public async Task<(bool Success, string Message)> DeleteDomain(int id)
        {
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/DeleteDomain/{id}", new { });

                res.EnsureSuccessStatusCode();

                var response = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

                return (response.IsSuccess, response.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<ApiResponse<TrainingQuizQuestion>> DeleteTrainingQuizQuestion(int id)
        {
            var result = new ApiResponse<TrainingQuizQuestion>();
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/DeleteTrainingQuizQuestion/{id}", new { });
                res.EnsureSuccessStatusCode();
                var json = await res.Content.ReadFromJsonAsync<ApiResponse<TrainingQuizQuestion>>();
                return json;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                return result;
            }
        }

        #region #OnPremise AD
        public async Task<Dictionary<string, List<OnPremiseADUsers>>> GetAllOnPremiseADGroups()
        {
            Dictionary<string, List<OnPremiseADUsers>> data = new();

            try
            {
                var res = await HttpClient.GetAsync("api/Tenant/GetOnPremiseADGroups");

                res.EnsureSuccessStatusCode();

                data = await res.Content.ReadFromJsonAsync<Dictionary<string, List<OnPremiseADUsers>>>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return data;
        }

        public async Task<List<OnPremiseADUsers>> GetOnPremiseUsersByADGroup(string groupName)
        {
            List<OnPremiseADUsers> data = new();

            try
            {
                var res = await HttpClient.PostAsJsonAsync("api/Tenant/GetOnPremiseUsersByADGroup", groupName);

                res.EnsureSuccessStatusCode();

                data = await res.Content.ReadFromJsonAsync<List<OnPremiseADUsers>>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return data;
        }

        #endregion

        #region Report
        public async Task<IEnumerable<CampaignLog>> BarChartDrillDownReportCount(int campaignId)
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/BarChartDrillDownReportCount/{campaignId}");

                res.EnsureSuccessStatusCode();

                return await res.Content.ReadFromJsonAsync<IEnumerable<CampaignLog>>();


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<ReportDataCounts>> PieChartDrillDownReportCount(DrillDownReportCountParameter filters)
        {
            var res = await HttpClient.PostAsJsonAsync("api/tenant/PieChartDrillDownReportCount", filters);

            res.EnsureSuccessStatusCode();

            var data = await res.Content.ReadFromJsonAsync<IEnumerable<ReportDataCounts>>();
            return data;
        }

        #endregion
    }
}
