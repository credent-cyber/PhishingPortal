using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Services.Utilities.Helper;
using Serilog.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Utilities.WeeklyReport
{
    internal class WeeklySummaryReports : IWeeklySummaryReports
    {
        private readonly List<IObserver<EmailCampaignInfo>> observers;

        private string BaseUrl = "http://localhost:7081/cmp";
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<WeeklySummaryReports> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }

        public WeeklySummaryReports(ILogger<WeeklySummaryReports> logger,
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

                    var allActiveCampaigns = dbContext.Campaigns.Include(o => o.Detail).Include(o => o.Schedule)
                                            .Where(o => (o.State == CampaignStateEnum.Published || o.State == CampaignStateEnum.InProgress) && o.IsActive
                                                && o.Detail.Type == CampaignType.Email).ToList();

                    DateTime startDate = GetStartOfWeek(DateTime.Today, DayOfWeek.Monday);
                    DateTime endDate = startDate.AddDays(7);

                    var logsInWeek = dbContext.CampaignLogs.Where(log => log.SentOn >= startDate && log.SentOn < endDate);
                    int totalIsHit = logsInWeek.Count(log => log.IsHit);
                    int totalLogs = logsInWeek.Count();
                    int totalReported = logsInWeek.Count(log => log.IsReported);

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }
        private DateTime GetStartOfWeek(DateTime currentDate, DayOfWeek startOfWeek)
        {
            int diff = (7 + (currentDate.DayOfWeek - startOfWeek)) % 7;
            return currentDate.AddDays(-1 * diff).Date;
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


                        var ecinfo = new EmailCampaignInfo()
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


        public IDisposable Subscribe(IObserver<EmailCampaignInfo> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);

            }
            return new Unsubscriber<EmailCampaignInfo>(observers, observer);
        }


    }
}
