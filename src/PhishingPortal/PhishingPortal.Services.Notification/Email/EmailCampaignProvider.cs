﻿using PhishingPortal.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using PhishingPortal.Dto;
using PhishingPortal.Common;
using PhishingPortal.Services.Notification.Monitoring;
using PhishingPortal.Services.Notification.Helper;
using PhishingPortal.Dto.Extensions;
using Humanizer;
using Serilog.Core;

namespace PhishingPortal.Services.Notification.Email
{

    public class EmailCampaignProvider : IEmailCampaignProvider
    {
        private readonly List<IObserver<EmailCampaignInfo>> observers;

        private string BaseUrl = "http://localhost:7081/cmp";

        public EmailCampaignProvider(ILogger<EmailCampaignProvider> logger,
            IEmailClient emailSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager, ILicenseEnforcementService licenseEnforcement)
        {
            Logger = logger;
            EmailSender = emailSender;
            Tenant = tenant;
            ConnManager = connManager;
            LicenseEnforcement = licenseEnforcement;
            BaseUrl = config.GetValue<string>("BaseUrl");
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            observers = new();
        }

        /// <summary>
        /// ExecuteAsync
        /// </summary>
        /// <param name="stopppingToken"></param>
        /// <returns></returns>
        public async Task CheckAndPublish(CancellationToken stopppingToken)
        {
            await Task.Run(async () =>
            {

                try
                {
                    var dbContext = ConnManager.GetContext(Tenant.UniqueId);

                    if (!LicenseEnforcement.HasValidLicense(dbContext, Dto.Subscription.AppModules.EmailCampaign))
                        return;

                    var allActiveCampaigns = dbContext.Campaigns.Include(o => o.Detail).Include(o => o.Schedule)
                                            .Where(o => (o.State == CampaignStateEnum.Published || o.State == CampaignStateEnum.InProgress) && o.IsActive
                                                && o.Detail.Type == CampaignType.Email).ToList();

                    MarkExpiredOrCompleted(ConnManager.GetContext(Tenant.UniqueId), allActiveCampaigns);


                    allActiveCampaigns = allActiveCampaigns.Where(o =>
                                o.Schedule.IsScheduledNow() &&
                                o.State == CampaignStateEnum.Published).ToList();

                    foreach (var campaign in allActiveCampaigns)
                    {
                        campaign.State = CampaignStateEnum.InProgress;
                        dbContext.Update(campaign);
                        dbContext.SaveChanges();

                        await Send(campaign, dbContext, Tenant.UniqueId);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }

        /// <summary>
        /// Publish campaign event to the observers
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
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


        private void MarkExpiredOrCompleted(TenantDbContext dbContext, List<Campaign> allActiveCampaigns)
        {
            try
            {
                var allCampaignScheduleExpired = allActiveCampaigns.Where(o => (!o.Schedule.IsScheduledNow() || o.Schedule.ScheduleType == ScheduleTypeEnum.NoSchedule)
                       && o.State == CampaignStateEnum.InProgress);

                foreach (var c in allCampaignScheduleExpired)
                {
                    var allCount = dbContext.CampaignRecipients.Count(r => r.CampaignId == c.Id);
                    var allLogs = dbContext.CampaignLogs.Where(o => o.CampaignId == c.Id);
                    var allSentOrCompleted = allLogs.Count(o => o.Status == CampaignLogStatus.Sent.ToString() || o.Status == CampaignLogStatus.Completed.ToString());

                    if (allCount > 0)
                    {
                        var percentSent = (allSentOrCompleted / allCount) * 100;

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
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"Error while marking campaigns completed");
            }
        }


        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<EmailCampaignProvider> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }
        public ILicenseEnforcementService LicenseEnforcement { get; }
    }
}
