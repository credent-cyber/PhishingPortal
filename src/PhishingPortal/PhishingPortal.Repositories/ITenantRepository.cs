using PhishingPortal.Dto;
using PhishingPortal.Dto.Dashboard;

namespace PhishingPortal.Repositories
{
    public interface ITenantRepository
    {
        Task<Tuple<bool,string>> CampaignHit(string key);
        Task<ApiResponse<string>> CampaignSpamReport(string key);
        Task<IEnumerable<Campaign>> GetAllCampaigns(int pageIndex, int pageSize);
        Task<IEnumerable<CampaignTemplate>> GetAllTemplates(int pageIndex = 0, int pageSize = 10);
        Task<List<CampaignTemplate>> GetAllTemplates(CampaignType? type);
        Task<Campaign> GetCampaignById(int id);
        Task<CategoryWisePhishingTestData> GetCategoryWisePhishingReport(DateTime start, DateTime end);
        Task<ConsolidatedPhishingStats> GetLastPhishingStatics();
        Task<MonthlyPhishingBarChart> GetMonthlyBarChart(int year);
        Task<List<CampaignRecipient>> GetRecipientByCampaignId(int campaignId);
        Task<List<RecipientGroup>> GetRecipientGroups(bool adGroupOnly = false);
        Task<List<Recipient>> GetRecipientsByGroupId(int groupId);
        Task<CampaignTemplate> GetTemplateById(int id);
        Task<List<Recipient>> ImportAdGroupMembers(RecipientGroup group, List<Recipient> recipients);
        Task<List<RecipientImport>> ImportRecipientAsync(int campaignId, List<RecipientImport> data);
        Task<Campaign> UpsertCampaign(Campaign campaign);
        Task<CampaignTemplate> UpsertTemplate(CampaignTemplate template);

        Task<IEnumerable<CampaignLog>> GetCampaignLogs(List<string> query);
        Task<Training> UpsertTraining(Training training);
        Task<Training> GetTrainingById(int id);
        Task<List<RecipientImport>> ImportTrainingRecipient(int trainingId, List<RecipientImport> data);
        Task<List<TrainingRecipients>> GetRecipientByTrainingId(int trainingId);
        Task<Tuple<bool, string>> Training(string key);
        Task<MonthlyTrainingBarChart> GetTrainingReportData(int year);
        Task<TrainingStatics> GetLastTrainingStatics();
        Task<Campaign> GetCampaignByName(string name);
        Task<List<int>> GetYearList();
        Task<List<Training>> GetAllTraining();
        Task<List<TrainingQuiz>> UpsertTrainingQuiz(List<TrainingQuiz> dtos);
        Task<IEnumerable<TrainingQuiz>> GetTrainingQuizById(int id);
        Task<IEnumerable<TrainingQuiz>> GetQuizByTrainingId(int trainingId);
      
    }
}