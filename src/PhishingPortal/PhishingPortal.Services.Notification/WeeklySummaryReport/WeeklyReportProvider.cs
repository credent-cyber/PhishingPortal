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

                    // Get the start date and end date of the current week
                    DateTime currentDate = DateTime.Now.Date;
                    DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                    DateTime endOfWeek = startOfWeek.AddDays(6);

                    // Filter CompletedCampaigns based on the current week's dates
                    var CompletedCampaigns = dbContext.Campaigns
                        .Include(o => o.Detail)
                        .Where(o => o.State == CampaignStateEnum.Completed &&
                                    o.CreatedOn >= startOfWeek &&
                                    o.CreatedOn <= endOfWeek)
                        .ToList();

                    var campignLogs = dbContext.CampaignLogs.Where(o => (o.CreatedOn >= startOfWeek && o.CreatedOn <= endOfWeek)||(o.ModifiedOn >= startOfWeek && o.ModifiedOn <= endOfWeek)).Distinct();

                    WeeklyReportInfo weeklyReportInfo = new WeeklyReportInfo();
                    weeklyReportInfo.CampaignStatistics.EmailStatistics.TotalCampaigns = CompletedCampaigns.Where(o => o.Detail.Type == CampaignType.Email).Count();
                    weeklyReportInfo.CampaignStatistics.SmsStatistics.TotalCampaigns = CompletedCampaigns.Where(o => o.Detail.Type == CampaignType.Sms).Count();
                    weeklyReportInfo.CampaignStatistics.WhatsappStatistics.TotalCampaigns = CompletedCampaigns.Where(o => o.Detail.Type == CampaignType.Whatsapp).Count();

                    foreach (var campaign in CompletedCampaigns.Where(o=>o.Detail.Type ==CampaignType.Email))
                    {
                        //await Send(campaign, dbContext, Tenant.UniqueId);
                        campignLogs = campignLogs.Where(o => o.CampaignId == campaign.Id);
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


                        

                       // o.OnNext(ecinfo);
                    }
                };
            }
            catch (Exception ex)
            {
              
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
