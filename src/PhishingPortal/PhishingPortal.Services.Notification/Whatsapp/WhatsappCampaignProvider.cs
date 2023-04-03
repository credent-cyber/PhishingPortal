using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Extensions;
using PhishingPortal.Services.Notification.Helper;
using PhishingPortal.Services.Notification.Sms;

namespace PhishingPortal.Services.Notification.Whatsapp
{
    public class WhatsappCampaignProvider : IWhatsappCampaignProvider
    {
        private readonly List<IObserver<WhatsappCampaignInfo>> observers;

        private string BaseUrl = "http://localhost:7081/cmp";
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger Logger { get; }
        public IWhatsappGatewayClient WaSender { get; }
        public IConfiguration Config { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }

        public WhatsappCampaignProvider(ILogger logger,
            IWhatsappGatewayClient waSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            WaSender = waSender;
            Config = config;
            Tenant = tenant;
            ConnManager = connManager;

            BaseUrl = config.GetValue<string>("BaseUrl");
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            observers = new();
        }
        public async Task CheckAndPublish(CancellationToken stopppingToken)
        {
            await Task.Run(async () =>
            {

                try
                {

                    var dbContext = ConnManager.GetContext(Tenant.UniqueId);
                    dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                    var campaigns = dbContext.Campaigns.Include(o => o.Detail).Include(o => o.Schedule)
                                  .Where(o => o.IsActive && o.Detail.Type == CampaignType.Whatsapp).ToList();


                    var CampaignsToBeCompleted = campaigns.Where(o => o.State == CampaignStateEnum.InProgress)
                                                          .OrderBy(o => o.Id).FirstOrDefault();
                    if (CampaignsToBeCompleted != null)
                    {
                        //var total = dbContext.CampaignLogs.Where(o => o.CampaignId == CampaignsToBeCompleted.Id);
                        //int totalSent = total.Count(o => o.Status == CampaignLogStatus.Sent.ToString());
                        //decimal percentageSent = totalSent == 0 ? 0 : decimal.Divide(totalSent, total.Count()) * 100;

                        int total = dbContext.CampaignRecipients.Count(o => o.CampaignId == CampaignsToBeCompleted.Id);
                        int totalSent = dbContext.CampaignLogs.Count(o => o.CampaignId == CampaignsToBeCompleted.Id);
                        decimal percentageSent = totalSent == 0 ? 0 : decimal.Divide(totalSent, total) * 100;

                        if (percentageSent > 98 && CampaignsToBeCompleted.State != CampaignStateEnum.Completed)
                        {
                            CampaignsToBeCompleted.State = CampaignStateEnum.Completed;
                            dbContext.Update(CampaignsToBeCompleted);
                            dbContext.SaveChanges();
                        }
                    }


                    var campaign = campaigns.Where(o => o.State == CampaignStateEnum.Published && o.Schedule.IsScheduledNow()).OrderBy(o => o.Id).FirstOrDefault();
                    if (campaign != null)
                    {
                        campaign.State = CampaignStateEnum.InProgress;
                        dbContext.Update(campaign);
                        dbContext.SaveChanges();

                        await QueueSms(campaign, dbContext, Tenant.UniqueId);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }

        public IDisposable Subscribe(IObserver<WhatsappCampaignInfo> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);

            }
            return new Unsubscriber<WhatsappCampaignInfo>(observers, observer);
        }

        protected async Task QueueSms(Campaign campaign, TenantDbContext dbContext, string tenantIdentifier)
        {
            try
            {

                var template = dbContext.CampaignTemplates.Find(campaign.Detail.CampaignTemplateId);

                if (template == null)
                    throw new Exception("Template not found");


                var recipients = dbContext.CampaignRecipients
                    .Include(o => o.Recipient).Where(o => o.CampaignId == campaign.Id);

                foreach (var r in recipients)
                {
                    if (string.IsNullOrEmpty(r.Recipient.Mobile))
                        continue;

                    foreach (var o in observers)
                    {

                        var timestamp = DateTime.Now;
                        var key = $"{campaign.Id}-{r.Recipient.Email}-{timestamp}".ComputeMd5Hash().ToLower();
                        var returnUrl = $"{BaseUrl}/{tenantIdentifier}/{key}";
                        var content = template.Content.Replace("###RETURN_URL###", returnUrl);

                        // TODO: calculate short urls
                        var smsInfo = new WhatsappCampaignInfo()
                        {
                            Tenantdentifier = tenantIdentifier,
                            SmsRecipient = r.Recipient.Mobile,
                            From = campaign.FromEmail,
                            SmsContent = content,
                            LogEntry = new CampaignLog
                            {
                                SecurityStamp = key,
                                CampaignId = campaign.Id,
                                IsHit = false,
                                CreatedBy = "system",
                                CreatedOn = timestamp,
                                ReturnUrl = returnUrl,
                                RecipientId = r.Id,
                                CampignType = campaign.Detail.Type.ToString(),
                                SentBy = "system",
                                SentOn = timestamp,
                                Status = CampaignLogStatus.Sent.ToString()
                            }
                        };

                        o.OnNext(smsInfo);
                    }
                };

                var c = dbContext.Campaigns.Find(campaign.Id);
                c.State = CampaignStateEnum.Completed;
                dbContext.Update(c);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                campaign.State = CampaignStateEnum.Aborted;
                dbContext.Update(campaign);
                dbContext.SaveChanges();
                Logger.LogCritical(ex, $"Error sending WHATSAPP campaign - {ex.Message}, StackTrace :{ex.StackTrace}");
            }

            await Task.CompletedTask;
        }
    }
}