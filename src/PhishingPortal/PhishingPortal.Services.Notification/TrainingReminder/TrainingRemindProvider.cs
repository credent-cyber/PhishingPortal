using IdentityModel;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Email;
using PhishingPortal.Services.Notification.Helper;
using PhishingPortal.Services.Notification.WeeklySummaryReport;
using System.Linq;

namespace PhishingPortal.Services.Notification.TrainingReminder
{
    public class TrainingRemindProvider : ITrainingRemindProvider
    {
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<TrainingRemindProvider> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }

        private readonly List<IObserver<TrainingRemindInfo>> observers;

        private string _trainingRemindTemplatePath = "";

        private string BaseUrl = "https://phishsims.com/cmp";
        public TrainingRemindProvider(ILogger<TrainingRemindProvider> logger,
          IEmailClient emailSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            Tenant = tenant;
            ConnManager = connManager;
            BaseUrl = config.GetValue<string>("BaseUrl");
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            _trainingRemindTemplatePath = config.GetValue<string>("TrainingRemindTemplatePath");
            observers = new();
        }

        public async Task CheckAndPublish(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    var dbContext = ConnManager.GetContext(Tenant.UniqueId);

                    // Get current month start and end dates
                    DateTime currentDate = DateTime.Now.Date;
                    DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    // Query to find recipients whose training is not completed within the current month
                    var recipientsToRemind = dbContext.TrainingLog
                        .Where(t => t.Status != "Completed" && t.SentOn >= startOfMonth && t.SentOn <= endOfMonth)
                        .Join(dbContext.Recipients,
                              tl => tl.ReicipientID,
                              r => r.Id,
                              (tl, r) => new { TrainingLog = tl, Recipient = r })
                        .Select(j => new
                        {
                            j.Recipient.EmployeeCode,
                            j.Recipient.Name,
                            j.Recipient.Email,
                            j.Recipient.Mobile,
                            j.Recipient.WhatsAppNo,
                            j.Recipient.Branch,
                            j.Recipient.Department,
                            j.Recipient.Address,
                            j.Recipient.DateOfBirth,
                            j.Recipient.LastWorkingDate,
                            j.Recipient.IsActive,
                            j.Recipient.IsADUser,
                            j.Recipient.IsOnPremiseADUser,
                            j.TrainingLog.TrainingType,
                            j.TrainingLog.PercentCompleted,
                            j.TrainingLog.Status,
                            j.TrainingLog.SecurityStamp,
                            j.TrainingLog.Url,
                            j.TrainingLog.SentOn,
                            j.TrainingLog.UniqueID,
                            j.TrainingLog.CampaignLogID,
                            j.TrainingLog.RecipientName,
                            j.TrainingLog.TrainingName
                        })
                        .ToList();

                    //_trainingRemindTemplatePath = @"C:\Users\ChhaganSinha\Downloads\apex\WeeklyReportTemplate - Copy.html";
                    var templateDirectory = Path.GetDirectoryName(_trainingRemindTemplatePath);
                    if (!Directory.Exists(templateDirectory))
                        throw new Exception("Template directory does not exist: " + templateDirectory);

                    if (!File.Exists(_trainingRemindTemplatePath))
                        throw new Exception("Template File doesn't exist");

                    var content = File.ReadAllText(_trainingRemindTemplatePath);

                    foreach (var recipient in recipientsToRemind)
                    {
                        content = content.Replace("###RecipientName###", recipient.Email.Split('@')[0]).Replace("###TrainingLink###", recipient.Url).Replace("###DueDate###", DateTime.Now.AddDays(1).ToShortDateString());

                        // Create a new instance of TrainingRemindInfo and initialize the EmailDetails property
                        TrainingRemindInfo trainingRemindInfo = new TrainingRemindInfo
                        {
                            EmailDetails = new EmailDetails
                            {
                                Recipient = recipient.Email,
                                Subject = "Reminder: Complete Your Phishing Simulation Training",
                                Content = content,

                            },

                        };
                        // Send the reminder
                        Send(trainingRemindInfo, dbContext);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }
            });
        }



        protected async Task Send(TrainingRemindInfo trainingRemindInfo, TenantDbContext dbContext)
        {
            try
            {

                var settings = dbContext.Settings.ToList();
                var isWeeklyReportEnable = settings.Where(x => x.Key == Constants.Keys.TrainingReminder_IsEnabled).FirstOrDefault().Value;

                foreach (var o in observers)
                {
                    o.OnNext(trainingRemindInfo);
                }
            }
            catch (Exception ex)
            {

                Logger.LogCritical(ex, $"Error executing campaign - {ex.Message}, StackTrace :{ex.StackTrace}");
            }

            await Task.CompletedTask;
        }

        public IDisposable Subscribe(IObserver<TrainingRemindInfo> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);

            }
            return new Unsubscriber<TrainingRemindInfo>(observers, observer);
        }
    }
}
