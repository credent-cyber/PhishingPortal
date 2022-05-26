using Microsoft.Extensions.Logging;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using Microsoft.EntityFrameworkCore;

namespace PhishingPortal.Repositories
{
    public class TenantRepository : BaseRepository
    {
        public TenantRepository(ILogger logger, TenantDbContext dbContext)
            : base(logger)
        {
            TenantDbCtx = dbContext;
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaigns(int pageIndex = 0, int pageSize = 10)
        {
            var result = Enumerable.Empty<Campaign>();

            result = TenantDbCtx.Campaigns.Include(o => o.Schedule)
                .Skip(pageIndex * pageSize).Take(pageSize);

            return result;
        }

        public async Task<Campaign> GetCampaignById(int id)
        {
            Campaign result = null;

            result = TenantDbCtx.Campaigns.Include(o => o.Schedule).Include(o => o.Detail)
                .FirstOrDefault(o => o.Id == id);

            return result;
        }

        public async Task<Campaign> UpsertCampaign(Campaign campaign)
        {
            Campaign result = null;

            if (campaign == null)
                throw new ArgumentNullException("Invalid campaign data");

            campaign.IsActive = true;

            if (campaign.Id > 0)
            {
                campaign.ModifiedOn = DateTime.Now;
                TenantDbCtx.Campaigns.Update(campaign);
            }
            else
            {
                campaign.CreatedOn = DateTime.Now;

                TenantDbCtx.Campaigns.Add(campaign);
            }

            TenantDbCtx.SaveChanges();

            return campaign;
        }

        public async Task<IEnumerable<CampaignTemplate>> GetAllTemplates(int pageIndex = 0 , int pageSize = 10)
        {
            IEnumerable<CampaignTemplate> result = null;

            result = TenantDbCtx.CampaignTemplates.Skip(pageIndex * pageSize).Take(pageSize);

            return result;
        }


        public async Task<List<CampaignRecipient>> GetRecipientByCampaignId(int campaignId)
        {
            var result = TenantDbCtx.CampaignRecipients.Include(o => o.Recipient).Where(o => o.CampaignId == campaignId);

            return await Task.FromResult(result.ToList());
        }

        public async Task<List<RecipientImport>> ImportRecipientAsync(int campaignId, List<RecipientImport> data)
        {
            var hasChanges = false;
            foreach (var r in data)
            {
                if (!TenantDbCtx.Recipients.Any(o => o.Email == r.Email || o.Mobile == r.Mobile))
                {
                    var recipient = new Recipient
                    {
                        Email = r.Email,
                        Mobile = r.Mobile,
                        Name = r.Name,
                        WhatsAppNo = r.Mobile
                    };
                    r.ValidationErrMsg = string.Empty;
                    hasChanges = true;

                    TenantDbCtx.CampaignRecipients.Add(new CampaignRecipient
                    {
                        CampaignId = campaignId,
                        Recipient = recipient,
                        RecipientGroupId = null,
                    });

                }
                else
                {
                    var recipient = TenantDbCtx.Recipients
                        .FirstOrDefault(o => o.Email == r.Email || o.Mobile == r.Mobile);

                    recipient.Mobile = r.Mobile;
                    recipient.Name = r.Name;
                    recipient.Email = r.Email;

                    TenantDbCtx.Update(recipient);
                    hasChanges = true;
                    if (!TenantDbCtx.CampaignRecipients.Any(o => o.CampaignId == campaignId && o.RecipientId == recipient.Id))
                    {
                        TenantDbCtx.CampaignRecipients.Add(new CampaignRecipient
                        {
                            CampaignId = campaignId,
                            Recipient = recipient,
                            RecipientGroupId = null,
                        });
                    }
                }

            }

            if (hasChanges)
                TenantDbCtx.SaveChanges();
            else
            {
                throw new InvalidOperationException("No changes");
            }

            return await Task.FromResult(data);
        }

        public TenantDbContext TenantDbCtx { get; }

        public async Task<List<CampaignTemplate>> GetAllTemplates(CampaignType? type)
        {

            var templates = TenantDbCtx.CampaignTemplates.AsQueryable();

            if (type.HasValue)
            {
                templates = templates.Where(o => o.Type == type.Value);
            }

            return await templates.ToListAsync();
        }

        public async Task<CampaignTemplate> GetTemplateById(int id)
        {
            var template = TenantDbCtx.CampaignTemplates.FirstOrDefault(o => o.Id == id);
            return await Task.FromResult(template);
        }

        public async Task<CampaignTemplate> UpsertTemplate(CampaignTemplate template)
        {
            try
            {
                if (template.Id > 0)
                {
                    TenantDbCtx.Update(template);
                    TenantDbCtx.SaveChanges();
                }
                else
                {
                    TenantDbCtx.Add(template);
                    TenantDbCtx.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return template;
        }

        public async Task<bool> CampaignHit(string key)
        {
            var status = CampaignLogStatus.Sent.ToString();
            var campaignLog = TenantDbCtx.CampaignLogs
                .FirstOrDefault(o => o.SecurityStamp == key 
                                    && o.IsHit == false && o.Status == status);
            if (campaignLog == null)
                throw new Exception("Invalid Url");

            campaignLog.Status = CampaignLogStatus.Completed.ToString();
            campaignLog.IsHit = true;
            campaignLog.ModifiedOn = DateTime.Now;
            campaignLog.ModifiedBy = nameof(CampaignHit);

            TenantDbCtx.Update(campaignLog);
            TenantDbCtx.SaveChanges();

            return await Task.FromResult(true);
 
        }
    }
}