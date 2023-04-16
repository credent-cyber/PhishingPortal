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

namespace PhishingPortal.Services.Notification.Trainings
{
    public class TrainingProvider : ITrainingProvider
    {
        private readonly List<IObserver<TraininigInfo>> observers;

        private string TrainingBaseUrl = "http://localhost:7081/training";
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<ITrainingProvide> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }
        public IEmailTemplateProvider EmailTemplateProvider { get; }

        public TrainingProvider(ILogger<TrainingProvider> logger,
           IEmailClient emailSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager, IEmailTemplateProvider emailTemplateProvider)
        {
            Logger = logger;
            EmailSender = emailSender;
            Tenant = tenant;
            ConnManager = connManager;
            EmailTemplateProvider = emailTemplateProvider;
            TrainingBaseUrl = config.GetValue<string>("TrainingBaseUrl");
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


                    var training = dbContext.Training.Include(o => o.TrainingSchedule)
                                            .Where(o => (o.State == TrainingState.Published || o.State == TrainingState.InProgress) && o.IsActive).ToList();

                    MarkExpiredOrCompleted(dbContext, training);

                    training = training.Where(o => o.TrainingSchedule.IsScheduledNow() && o.State == TrainingState.Published).ToList();

                    foreach (var t in training)
                    {
                        t.State = TrainingState.InProgress;
                        dbContext.SaveChanges();

                        await Send(t, dbContext, Tenant.UniqueId);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }

        protected async Task Send(Training training, TenantDbContext DataContext, string tenantIdentifier)
        {
            try
            {
                var recipients = DataContext.TrainingRecipient.Include(o => o.AllTrainingRecipient).Where(o => o.TrainingId == training.Id);
                var uniqueID = Guid.NewGuid().ToString();
                foreach (var r in recipients)
                {
                    if (string.IsNullOrEmpty(r.AllTrainingRecipient.Email))
                        continue;

                    foreach (var o in observers)
                    {

                        var timestamp = DateTime.Now;
                        var key = $"{training.Id}-{r.AllTrainingRecipient.Email}-{timestamp}".ComputeMd5Hash().ToLower();
                        var returnUrl = $"{TrainingBaseUrl}/oidc/challenge?returnUrl=/training/detail/{uniqueID}&provider=Microsoft";

                        // TODO: use training template
                        var parameter = new Dictionary<string, string>();
                        parameter.Add("###LINK###", returnUrl);
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
                                Url = returnUrl,
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
                var allCount = dbContext.TrainingRecipient.Count(o =>o.TrainingId == t.Id);
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
