using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Email;
using PhishingPortal.Services.Notification.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public class WeeklyReportProvider : IWeeklyReportProvider
    {
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<WeeklyReportProvider> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }

        private readonly List<IObserver<WeeklyReportInfo>> observers;

        private string BaseUrl = "http://localhost:7081/cmp";

        public WeeklyReportProvider(ILogger<WeeklyReportProvider> logger,
            IEmailClient emailSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
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

                    var CompletedCampaigns = dbContext.Campaigns.Include(o => o.Detail).Include(o => o.Schedule)
                                            .Where(o => (o.State == CampaignStateEnum.Completed) && o.IsActive).ToList();


                    foreach (var campaign in CompletedCampaigns)
                    {
                        await Send(campaign, dbContext, Tenant.UniqueId);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }

        protected async Task Send(Campaign campaign, TenantDbContext dbContext, string tenantIdentifier)
        {
            try
            {

                var template = dbContext.CampaignTemplates.Find(campaign.Detail.CampaignTemplateId);

                if (template == null)
                    throw new Exception("Template not found");

                var recipients = dbContext.CampaignRecipients.Include(o => o.Recipient).Where(o => o.CampaignId == campaign.Id);

                foreach (var r in recipients)
                {
                    if (string.IsNullOrEmpty(r.Recipient.Email))
                        continue;

                    foreach (var o in observers)
                    {

                        var timestamp = DateTime.Now;
                        var key = $"{campaign.Id}-{r.Recipient.Email}-{timestamp}".ComputeMd5Hash().ToLower();
                        var returnUrl = $"{BaseUrl}/{tenantIdentifier}/{key}";
                        var content = template.Content.Replace("###RETURN_URL###", returnUrl);

                        // TODO: calculate short urls


                        var ecinfo = new WeeklyReportInfo()
                        {
                            Tenantdentifier = tenantIdentifier,
                            EmailRecipients = r.Recipient.Email,
                            EmailSubject = campaign.Subject,
                            EmailContent = content,
                            EmailFrom = campaign.FromEmail,
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

                        o.OnNext(ecinfo);
                    }
                };
            }
            catch (Exception ex)
            {
                campaign.State = CampaignStateEnum.Aborted;
                dbContext.Update(campaign);
                dbContext.SaveChanges();
                Logger.LogCritical(ex, $"Error executing campaign - {ex.Message}, StackTrace :{ex.StackTrace}");
            }

            await Task.CompletedTask;
        }


        public IDisposable Subscribe(IObserver<WeeklyReportInfo> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);

            }
            return new Unsubscriber<WeeklyReportInfo>(observers, observer);
        }
    }
}
