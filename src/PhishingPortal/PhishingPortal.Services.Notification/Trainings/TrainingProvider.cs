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

namespace PhishingPortal.Services.Notification.Trainings
{
    public class TrainingProvider : ITrainingProvider
    {
        private readonly List<IObserver<TraininigInfo>> observers;

        private string BaseUrl = "http://localhost:7081/training";
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<ITrainingProvide> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }


        public TrainingProvider(ILogger<TrainingProvider> logger,
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
                    dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;


                    var training = dbContext.Training.Include(o => o.TrainingSchedule)
                                            .Where(o => o.State == TrainingState.Published && o.IsActive).ToList();

                    training = training.Where(o => o.TrainingSchedule.IsScheduledNow()).ToList();

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

        protected async Task Send(Training training, TenantDbContext dbContext, string tenantIdentifier)
        {
            try
            {

                //  var template = dbContext.CampaignTemplates.Find(campaign.Detail.CampaignTemplateId);

                //if (template == null)
                //    throw new Exception("Template not found");


                var recipients = dbContext.TrainingRecipient.Include(o => o.AllTrainingRecipient).Where(o => o.TrainingId == training.Id);

                foreach (var r in recipients)
                {
                    if (string.IsNullOrEmpty(r.AllTrainingRecipient.Email))
                        continue;

                    foreach (var o in observers)
                    {

                        var timestamp = DateTime.Now;
                        var key = $"{training.Id}-{r.AllTrainingRecipient.Email}-{timestamp}".ComputeMd5Hash().ToLower();
                        var returnUrl = $"{BaseUrl}/{tenantIdentifier}/{key}";
                        var content = training.Content.Replace("###RETURN_URL###", returnUrl);

                        // TODO: calculate short urls


                        var ecinfo = new TraininigInfo()
                        {
                            Tenantdentifier = tenantIdentifier,
                            TrainingRecipients = r.AllTrainingRecipient.Email,
                            TrainingSubject = "Phishshims Training",
                            TrainingContent = content,
                            TrainingFrom = "training@phishsims.com",
                            TrainingLogEntry = new TrainingLog()
                            {
                                SecurityStamp = key,
                                TrainingID = training.Id,
                                CreatedBy = "system",
                                CreatedOn = timestamp,
                                ReicipientID = r.AllTrainingRecipient.Id,
                                TrainingType = training.TrainingCategory,
                                SentOn = timestamp,
                                Status = CampaignLogStatus.Sent.ToString()
                            }
                        };

                        o.OnNext(ecinfo);
                    }
                };

                var c = dbContext.Training.Find(training.Id);
                c.State = TrainingState.Completed;
                dbContext.Update(c);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                training.State = TrainingState.Aborted;
                dbContext.Update(training);
                dbContext.SaveChanges();
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
    }
}
