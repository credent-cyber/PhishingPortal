using PhishingPortal.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Extensions;
using PhishingPortal.Common;
using PhishingPortal.Services.Notification.Helper;
using Microsoft.Extensions.Logging;
using PhishingPortal.Services.Notification.Email;
using AutoMapper.Configuration.Annotations;
using PhishingPortal.Services.Notification.EmailTemplate;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace PhishingPortal.Services.Notification.Trainings
{
    public class TrainingProvider : ITrainingProvider
    {
        private readonly List<IObserver<TraininigInfo>> observers;

        private string TrainingBaseUrl = "http://localhost:7081/training";
        private long CampaignTrainingHours = 24;
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<ITrainingProvide> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }
        public IEmailTemplateProvider EmailTemplateProvider { get; }
        public ILicenseEnforcementService LicenseEnforcement { get; }

        public TrainingProvider(ILogger<TrainingProvider> logger,
           IEmailClient emailSender, IConfiguration config, Tenant tenant, 
           ITenantDbConnManager connManager, IEmailTemplateProvider emailTemplateProvider, ILicenseEnforcementService licenseEnforcement)
        {
            Logger = logger;
            EmailSender = emailSender;
            Tenant = tenant;
            ConnManager = connManager;
            EmailTemplateProvider = emailTemplateProvider;
            LicenseEnforcement = licenseEnforcement;
            TrainingBaseUrl = config.GetValue<string>("TrainingBaseUrl");
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            CampaignTrainingHours = config.GetValue<long>("CampaignTrainingHours");

            if(CampaignTrainingHours == 0)
                CampaignTrainingHours = 24;

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

                    if (!LicenseEnforcement.HasValidLicense(dbContext, Dto.Subscription.AppModules.TrainingCampaign))
                        return;

                    var training = dbContext.Training.Include(o => o.TrainingSchedule)
                                            .Where(o => (o.State == TrainingState.Published || o.State == TrainingState.InProgress) && o.IsActive && !o.TrainingTrigger).ToList();

                    MarkExpiredOrCompleted(dbContext, training);

                    training = training.Where(o => o.TrainingSchedule.IsScheduledNow() && o.State == TrainingState.Published).ToList();

                    foreach (var t in training)
                    {
                        t.State = TrainingState.InProgress;
                        dbContext.SaveChanges();

                        await Send(t, dbContext, Tenant.UniqueId);
                    }
                    var dbContext2 = ConnManager.GetContext(Tenant.UniqueId);
                    await SendFailedCampaignTrainings(dbContext2, Tenant.UniqueId);
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }

        protected async Task SendFailedCampaignTrainings(TenantDbContext DataContext, string tenantIdentifier)
        {
            var failedCampaignLogs = DataContext.CampaignLogs.Where(o =>
                    (o.IsHit || o.IsDataEntered || o.IsMacroEnabled) &&
#if !DEBUG
                    o.ModifiedOn >= DateTime.Now.AddHours(-1 * CampaignTrainingHours) &&
#endif
                    (o.Status == CampaignLogStatus.Sent.ToString() || o.Status == CampaignLogStatus.Completed.ToString()));

            var failedCampaignTrainings = from cl in failedCampaignLogs.Include(o => o.Recipient).Include(o => o.Recipient.Recipient)
                            join tcm in DataContext.TrainingCampaignMapping.Include(o => o.Training) on cl.CampaignId equals tcm.CampaignId
                            select new {
                                tcm.Training,
                                tcm.Campaign,
                                CampaignLog = cl,
                                cl.Recipient
                            };

            

            foreach (var t in failedCampaignTrainings)
            {
                var uniqueID = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(t.Recipient.Recipient.Email))
                    continue;

                foreach (var o in observers)
                {
                    var timestamp = DateTime.Now;
                    var key = $"{t.Training.Id}-{t.Recipient.Recipient.Email}-{timestamp}".ComputeMd5Hash().ToLower();
                    var trainingUrl = $"{TrainingBaseUrl}{uniqueID}";

                    var parameter = new Dictionary<string, string>();
                    parameter.Add("###LINK###", trainingUrl);
                    parameter.Add("###NAME###", t.Recipient.Recipient.Name);

                    var content = EmailTemplateProvider.GetEmailBody(Constants.EmailTemplate.TRAINING_CAMPAIGN, parameter);
                    var ecinfo = new TraininigInfo()
                    {
                        Tenantdentifier = tenantIdentifier,
                        TrainingRecipients = t.Recipient.Recipient.Email,
                        TrainingSubject = t.Training.TrainingName,
                        TrainingContent = content,
                        TrainingFrom = "training@phishsims.com",
                        TrainingLogEntry = new TrainingLog()
                        {
                            SecurityStamp = key,
                            TrainingID = t.Training.Id,
                            CreatedBy = Constants.Literals.CREATED_BY,
                            CreatedOn = timestamp,
                            ReicipientID = t.Recipient.Recipient.Id,
                            TrainingType = t.Training.TrainingCategory,
                            SentOn = timestamp,
                            Status = TrainingLogStatus.Sent.ToString(),
                            Url = trainingUrl,
                            UniqueID = uniqueID,
                            CampaignLogID = t.CampaignLog.Id
                        }
                    };

                    o.OnNext(ecinfo);
                }
            }

            await Task.CompletedTask;
        }

        protected async Task Send(Training training, TenantDbContext DataContext, string tenantIdentifier)
        {
            try
            {
                var recipients = DataContext.TrainingRecipient.Include(o => o.AllTrainingRecipient).Where(o => o.TrainingId == training.Id);
              
                foreach (var r in recipients)
                {
                    var uniqueID = Guid.NewGuid().ToString();

                    if (string.IsNullOrEmpty(r.AllTrainingRecipient.Email))
                        continue;

                    foreach (var o in observers)
                    {

                        var timestamp = DateTime.Now;
                        var key = $"{training.Id}-{r.AllTrainingRecipient.Email}-{timestamp}".ComputeMd5Hash().ToLower();
                        var trainingUrl = $"{TrainingBaseUrl}{uniqueID}";

                        // TODO: use training template
                        var parameter = new Dictionary<string, string>();
                        parameter.Add("###LINK###", trainingUrl);
                        parameter.Add("###NAME###", r.AllTrainingRecipient.Name);

                        var content = EmailTemplateProvider.GetEmailBody(Constants.EmailTemplate.TRAINING_CAMPAIGN, parameter);

                        var ecinfo = new TraininigInfo()
                        {
                            Tenantdentifier = tenantIdentifier,
                            TrainingRecipients = r.AllTrainingRecipient.Email,
                            TrainingSubject = training.TrainingName,
                            TrainingContent = content,
                            TrainingFrom = "training@phishsims.com",
                            TrainingLogEntry = new TrainingLog()
                            {
                                SecurityStamp = key,
                                TrainingID = training.Id,
                                CreatedBy = Constants.Literals.CREATED_BY,
                                CreatedOn = timestamp,
                                ReicipientID = r.AllTrainingRecipient.Id,
                                TrainingType = training.TrainingCategory,
                                SentOn = timestamp,
                                Status = TrainingLogStatus.Sent.ToString(),
                                Url = trainingUrl,
                                UniqueID = uniqueID,
                            }
                        };

                        o.OnNext(ecinfo);
                    }
                };

                var c = DataContext.Training.Find(training.Id);
                c.State = TrainingState.Completed;
                DataContext.Update(c);
                DataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                training.State = TrainingState.Aborted;
                DataContext.Update(training);
                DataContext.SaveChanges();
                Logger.LogCritical(ex, $"Error executing campaign - {ex.Message}, StackTrace :{ex.StackTrace}");
            }

            await Task.CompletedTask;
        }

        public IDisposable Subscribe(IObserver<TraininigInfo> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);

            }
            return new Unsubscriber<TraininigInfo>(observers, observer);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(TraininigInfo value)
        {
            throw new NotImplementedException();
        }

        private static void MarkExpiredOrCompleted(TenantDbContext dbContext, List<Training> allActiveCampaigns)
        {
            var allTrainingExpired = allActiveCampaigns.Where(o => o.State == TrainingState.InProgress
                    && !o.TrainingSchedule.IsScheduledNow()
                    && o.TrainingSchedule.ScheduleType != ScheduleTypeEnum.NoSchedule);

            foreach (var t in allTrainingExpired)
            {
                var allCount = dbContext.TrainingRecipient.Count(o => o.TrainingId == t.Id);
                var allLogs = dbContext.TrainingLog.Where(c => c.TrainingID == c.Id);
                var allSent = allLogs.Count(o => o.Status == TrainingLogStatus.Sent.ToString());

                if (allCount > 0)
                {
                    var percentSent = (allSent / allCount) * 100;

                    if (percentSent >= 98)
                    {
                        t.State = TrainingState.Completed;
                    }
                    else
                    {
                        t.State = TrainingState.Faulted;
                    }

                    dbContext.Update(t);
                    dbContext.SaveChanges();
                }
                else
                {
                    t.State = TrainingState.Completed;
                    dbContext.Update(t);
                    dbContext.SaveChanges();
                }

            }
        }

    }
}
