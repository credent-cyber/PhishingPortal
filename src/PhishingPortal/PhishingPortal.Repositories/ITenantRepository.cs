using PhishingPortal.Dto;
using PhishingPortal.Dto.Dashboard;

namespace PhishingPortal.Repositories
{
    public interface ITenantRepository
    {
        Task<Tuple<bool,string>> CampaignHit(string key);
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

    }
}