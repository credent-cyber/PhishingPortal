using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Extensions;
using PhishingPortal.Services.Notification.Helper;

namespace PhishingPortal.Services.Notification.Sms
{
    public class SmsCampaignProvider : ISmsCampaignProvider
    {
        private readonly List<IObserver<SmsCampaignInfo>> observers;
        private string BaseUrl = "http://localhost:7081/cmp";
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public SmsCampaignProvider(ILogger logger,
            ISmsGatewayClient smsSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            SmsSender = smsSender;
            Config = config;
            Tenant = tenant;
            ConnManager = connManager;

            BaseUrl = config.GetValue<string>("BaseUrl");
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            observers = new();
        }

        public ILogger Logger { get; }
        public ISmsGatewayClient SmsSender { get; }
        public IConfiguration Config { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }

        public async Task CheckAndPublish(CancellationToken stopppingToken)
        {
            //await Task.Run(async () =>
            //{

                try
                {

                    var dbContext = ConnManager.GetContext(Tenant.UniqueId);
                    dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                    var allActiveCampaigns = dbContext.Campaigns.Include(o => o.Detail).Include(o => o.Schedule)
                                            .Where(o => ( o.State == CampaignStateEnum.Published || o.State == CampaignStateEnum.InProgress)&& o.IsActive
                                                && o.Detail.Type == CampaignType.Sms).ToList();

                    MarkExpiredOrCompleted(dbContext, allActiveCampaigns);

                    allActiveCampaigns = allActiveCampaigns.Where(o => o.Schedule.IsScheduledNow() &&
                                o.State == CampaignStateEnum.Published).ToList();

                    foreach (var campaign in allActiveCampaigns)
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

            //});
        }

        public IDisposable Subscribe(IObserver<SmsCampaignInfo> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);

            }
            return new Unsubscriber<SmsCampaignInfo>(observers, observer);
        }

        /// <summary>
        /// Publish campaign event to the observers
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
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
                        var smsInfo = new SmsCampaignInfo()
                        {
                            Tenantdentifier = tenantIdentifier,
                            SmsRecipient = r.Recipient.Mobile,
                            From = campaign.FromEmail,
                            SmsContent = content,
                            TemplateId = template.TemplateId,

                            LogEntry = new CampaignLog
                            {
                                SecurityStamp = key,
                                CampaignId = campaign.Id,
                                IsHit = false,
                                CreatedBy = "system",
                                CreatedOn = timestamp,
                                ReturnUrl = returnUrl,
                                RecipientId = r.Recipient.Id,
                                CampignType = campaign.Detail.Type.ToString(),
                                SentBy = "system",
                                SentOn = timestamp,
                                Status = CampaignLogStatus.Queued.ToString()
                            }
                        };

                        o.OnNext(smsInfo);
                    }
                };

                //var c = dbContext.Campaigns.Find(campaign.Id);
                //c.State = CampaignStateEnum.Completed;
                //dbContext.Update(c);
                //dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                campaign.State = CampaignStateEnum.Aborted;
                dbContext.Update(campaign);
                dbContext.SaveChanges();
                Logger.LogCritical(ex, $"Error sending sms campaign - {ex.Message}, StackTrace :{ex.StackTrace}");
            }

            await Task.CompletedTask;
        }

        private static void MarkExpiredOrCompleted(TenantDbContext dbContext, List<Campaign> allActiveCampaigns)
        {
            var allCampaignScheduleExpired = allActiveCampaigns.Where(o => (!o.Schedule.IsScheduledNow() || o.Schedule.ScheduleType == ScheduleTypeEnum.NoSchedule)
                && o.State == CampaignStateEnum.InProgress);

            foreach (var c in allCampaignScheduleExpired)
            {
                var allCount = dbContext.CampaignRecipients.Count(r => r.CampaignId == c.Id);
                var allLogs = dbContext.CampaignLogs.Where(acl => acl.CampaignId == c.Id);
                var allSent = allLogs.Count(o => o.Status == CampaignLogStatus.Sent.ToString());
             

                if (allCount > 0)
                {
                    var percentSent = (allSent / allCount) * 100;

                    if (percentSent >= 95)
                    {
                        c.State = CampaignStateEnum.Completed;
                    }
                    if (c.Schedule.ScheduleType == ScheduleTypeEnum.NoSchedule &&
                           c.State == CampaignStateEnum.InProgress)
                    {
                        var lastLogTime = dbContext.CampaignLogs.Where(o => o.CampaignId == c.Id)?.OrderByDescending(o => o.SentOn)
                            .Select(o => o.SentOn).FirstOrDefault();

                        if (lastLogTime.HasValue && (DateTime.Now - lastLogTime.Value).TotalMinutes > 15)
                        {
                            c.State = CampaignStateEnum.InComplete;
                        }

                    }
                    else if (c.Schedule.ScheduleType != ScheduleTypeEnum.NoSchedule && c.State == CampaignStateEnum.InProgress
                        && c.Schedule.ToActualScheduleType()?.GetElapsedTimeInMinutes() > 15)
                    {
                        c.State = CampaignStateEnum.InComplete;
                    }                  

                    dbContext.Update(c);
                    dbContext.SaveChanges();
                }
                else
                {
                    c.State = CampaignStateEnum.Completed;
                    dbContext.Update(c);
                    dbContext.SaveChanges();
                }

            }
        }

    }
}