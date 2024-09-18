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
//using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;

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
        private string _weeklyReportTemplatePath = "";

        public WeeklyReportProvider(ILogger<WeeklyReportProvider> logger,
            IEmailClient emailSender, IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            Tenant = tenant;
            ConnManager = connManager;
            BaseUrl = config.GetValue<string>("BaseUrl");
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            _weeklyReportTemplatePath = config.GetValue<string>("WeeklyReportTemplatePath");
            observers = new();
        }

        public async Task CheckAndPublish(CancellationToken stopppingToken)
        {
            WeeklyReportInfo weeklyReportInfo = new WeeklyReportInfo();

            await Task.Run(() =>
            {
                try
                {
                    var dbContext = ConnManager.GetContext(Tenant.UniqueId);

                    DateTime currentDate = DateTime.Now.Date;
                    DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                    DateTime endOfWeek = startOfWeek.AddDays(6);

                    var completedCampaigns = dbContext.Campaigns
                        .Include(o => o.Detail)
                        .Where(o => o.State == CampaignStateEnum.Completed &&
                                    o.CreatedOn >= startOfWeek &&
                                    o.CreatedOn <= endOfWeek)
                        .ToList();

                    var trainingLogs = dbContext.TrainingLog
                        .Where(o => o.CreatedOn >= startOfWeek &&
                                    o.CreatedOn <= endOfWeek);

                    var campaignLogs = dbContext.CampaignLogs
                        .Where(o => (o.CreatedOn >= startOfWeek && o.CreatedOn <= endOfWeek) ||
                                    (o.ModifiedOn >= startOfWeek && o.ModifiedOn <= endOfWeek))
                        .Distinct();

                    var RecipientsByCampaignlog = from log in campaignLogs
                                                  join crec in dbContext.Recipients on log.RecipientId equals crec.Id
                                                  select new { logEntry = log, Department = crec.Department };

                    // Grouping department-wise data for each campaign type
                    var departmentsHitsViaEmail = GroupDepartmentStatistics(RecipientsByCampaignlog, CampaignType.Email.ToString());
                    var departmentsHitsViaSms = GroupDepartmentStatistics(RecipientsByCampaignlog, CampaignType.Sms.ToString());
                    var departmentsHitsViaWhatsapp = GroupDepartmentStatistics(RecipientsByCampaignlog, CampaignType.Whatsapp.ToString());

                    weeklyReportInfo.TenantIdentifier = Tenant.UniqueId;
                    weeklyReportInfo.EmailDetails = new EmailDetails();
                    weeklyReportInfo.CampaignStatistics = new CampaignStatistics
                    {
                        EmailStatistics = MapDepartmentStatistics(departmentsHitsViaEmail),
                        SmsStatistics = MapDepartmentStatistics(departmentsHitsViaSms),
                        WhatsappStatistics = MapDepartmentStatistics(departmentsHitsViaWhatsapp),
                        departmentHitReport = new DepartmentHitReport
                        {
                            EmailCampaignDepartmentReport = departmentsHitsViaEmail,
                            SmsCampaignDepartmentReport = departmentsHitsViaSms,
                            WhatsappCampaignDepartmentReport = departmentsHitsViaWhatsapp
                        }
                    };
                    weeklyReportInfo.TrainingStatistics = new TrainingStatistics();


                    // Populate campaign statistics
                    PopulateCampaignStatistics(weeklyReportInfo.CampaignStatistics, completedCampaigns, campaignLogs);

                    // Populate training statistics
                    IQueryable<Recipient> recipients = dbContext.Recipients;
                    PopulateTrainingStatistics(weeklyReportInfo.TrainingStatistics, trainingLogs, recipients);
                    Send(weeklyReportInfo, dbContext);
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }
            });
        }
        private DepartmentReport MapDepartmentReport(IEnumerable<dynamic> departmentStatistics)
        {
            var departmentReport = new DepartmentReport();

            // Check if there are any department statistics available
            if (departmentStatistics.Any())
            {
                // Get the department with the highest number of hits
                var departmentWithMostHits = departmentStatistics.OrderByDescending(d => d.Hits).FirstOrDefault();

                // Assign properties to the department report
                departmentReport.DepartmentName = departmentWithMostHits.Department;
                departmentReport.TotalSent = departmentWithMostHits.TotalSent;
                departmentReport.TotalHits = departmentWithMostHits.Hits;
                departmentReport.TotalReported = departmentWithMostHits.Reported;
                departmentReport.PronePercentage = CalculatePronePercentage(departmentWithMostHits.Hits, departmentReport.TotalSent);
                departmentReport.MostPhishingDepartment = departmentWithMostHits.Department;
            }

            return departmentReport;
        }

        private List<DepartmentReport> GroupDepartmentStatistics(IEnumerable<dynamic> recipientsByCampaignlog, string campaignType)
        {
            var data = recipientsByCampaignlog
                .Where(a => a.logEntry.CampignType == campaignType)
                .GroupBy(i => i.Department, (key, entries) => new DepartmentReport
                {
                    DepartmentName = key ?? "UNKNOWN",
                    TotalHits = entries.Count(o => o.logEntry.IsHit),
                    TotalReported = entries.Count(o => o.logEntry.IsReported),
                    PronePercentage = CalculatePronePercentage(entries.Count(o => o.logEntry.IsHit), entries.Count())
                })
                .ToList();
            return data;
        }

        private decimal CalculatePronePercentage(int hits, int count)
        {
            if (count > 0)
            {
                return Math.Round(((decimal)hits / count) * 100, 2);
            }
            return 0;
        }

        private CampaignTypeStatistics MapDepartmentStatistics(List<DepartmentReport> departmentReports)
        {
            return new CampaignTypeStatistics
            {
                TotalCampaigns = departmentReports.Sum(x => x.TotalHits),
                TotalHits = departmentReports.Sum(x => x.TotalHits),
                TotalReported = departmentReports.Sum(x => x.TotalReported),
                PronePercentage = departmentReports.Any() ? departmentReports.Max(x => x.PronePercentage) : 0,
                MostPhishingDepartment = departmentReports.Any() ? departmentReports.OrderByDescending(x => x.PronePercentage).First().DepartmentName : "UNKNOWN"
            };
        }


        private void PopulateCampaignStatistics(CampaignStatistics campaignStatistics, List<Campaign> completedCampaigns, IQueryable<CampaignLog> campaignLogs)
        {
            campaignStatistics.EmailStatistics = PopulateCampaignTypeStatistics(completedCampaigns, campaignLogs, CampaignType.Email);
            campaignStatistics.SmsStatistics = PopulateCampaignTypeStatistics(completedCampaigns, campaignLogs, CampaignType.Sms);
            campaignStatistics.WhatsappStatistics = PopulateCampaignTypeStatistics(completedCampaigns, campaignLogs, CampaignType.Whatsapp);
        }

        private CampaignTypeStatistics PopulateCampaignTypeStatistics(List<Campaign> completedCampaigns, IQueryable<CampaignLog> campaignLogs, CampaignType type)
        {
            var typeCampaigns = completedCampaigns.Where(o => o.Detail.Type == type);
            int totalCampaigns = typeCampaigns.Count();
            int totalCampaignLogs = campaignLogs.Count();
            int totalHits = 0;
            int totalReported = 0;
            foreach (var campaign in typeCampaigns)
            {
                totalHits += campaignLogs.Count(o => o.CampaignId == campaign.Id && o.IsHit);
                totalReported += campaignLogs.Count(o => o.CampaignId == campaign.Id && o.IsReported);
            }
            decimal pronePercentage = 0;
            if (totalCampaignLogs > 0) // Only calculate prone percentage if there are hits
            {
                pronePercentage = Math.Round(((decimal)totalHits / totalCampaignLogs) * 100, 2);
            }
            return new CampaignTypeStatistics
            {
                TotalCampaigns = totalCampaigns,
                TotalHits = totalHits,
                TotalReported = totalReported,
                PronePercentage = pronePercentage
            };
        }

        private void PopulateTrainingStatistics(TrainingStatistics trainingStatistics, IQueryable<TrainingLog> trainingLogs, IQueryable<Recipient> recipients)
        {
            // Initialize the department-wise training statistics list
            trainingStatistics.departmentWiseTrainingStatistics = new List<DepartmentWiseTrainingStatistics>();

            // Group training logs by department
            var departmentTrainingLogs = from log in trainingLogs
                                         join recipient in recipients on log.ReicipientID equals recipient.Id
                                         group log by recipient.Department into departmentLogs
                                         select new
                                         {
                                             DepartmentName = departmentLogs.Key,
                                             Assigned = departmentLogs.Count(),
                                             Completed = departmentLogs.Count(x => x.Status == TrainingLogStatus.Completed.ToString()),
                                             InCompleted = departmentLogs.Count(x => x.Status == TrainingLogStatus.InProgress.ToString()),
                                             NotAttempted = departmentLogs.Count(x => x.Status == TrainingLogStatus.Assigned.ToString()),
                                             CompletionPercentage = departmentLogs.Any() ? Math.Round((decimal)departmentLogs.Count(x => x.Status == TrainingLogStatus.Completed.ToString()) / departmentLogs.Count() * 100, 2) : 0
                                         };

            // Populate the department-wise training statistics list
            foreach (var departmentLog in departmentTrainingLogs)
            {
                trainingStatistics.departmentWiseTrainingStatistics.Add(new DepartmentWiseTrainingStatistics
                {
                    DepartmentName = departmentLog.DepartmentName,
                    Assigned = departmentLog.Assigned,
                    Completed = departmentLog.Completed,
                    InCompleted = departmentLog.InCompleted,
                    NotAttempt = departmentLog.NotAttempted,
                    CompletionPercentage = departmentLog.CompletionPercentage
                });
            }

            // Populate total, completed, and incomplete counts
            trainingStatistics.Total = trainingLogs.Count();
            trainingStatistics.Completed = trainingLogs.Count(x => x.Status == TrainingLogStatus.Completed.ToString());
            trainingStatistics.Incomplete = trainingLogs.Count(x => x.Status == TrainingLogStatus.Assigned.ToString());

            // Calculate overall completion percentage
            if (trainingStatistics.Total > 0)
            {
                trainingStatistics.CompletionPercentage = Math.Round((decimal)trainingStatistics.Completed / trainingStatistics.Total * 100, 2);
            }
            else
            {
                trainingStatistics.CompletionPercentage = 0;
            }
        }



        protected async Task Send(WeeklyReportInfo weeklyReportInfo, TenantDbContext dbContext)
        {
            try
            {

                var settings = dbContext.Settings.ToList();
                var isWeeklyReportEnable = settings.Where(x => x.Key == Constants.Keys.WeeklyReport_IsEnabled).FirstOrDefault().Value;
                var Recipients = settings.Where(x => x.Key == Constants.Keys.WeeklyReport_Recipients).FirstOrDefault().Value;
                weeklyReportInfo.EmailDetails.Recipients = Recipients;

                var parameter = new Dictionary<string, string>();
                parameter.Add("###EmailTotalCount###", weeklyReportInfo.CampaignStatistics.EmailStatistics.TotalCampaigns.ToString());
                parameter.Add("###EmailHitCount###", weeklyReportInfo.CampaignStatistics.EmailStatistics.TotalHits.ToString());
                parameter.Add("###EmailTotalReported###", weeklyReportInfo.CampaignStatistics.EmailStatistics.TotalReported.ToString());
                parameter.Add("###EmailProne%###", weeklyReportInfo.CampaignStatistics.EmailStatistics.PronePercentage.ToString());

                parameter.Add("###SmsTotalCount###", weeklyReportInfo.CampaignStatistics.SmsStatistics.TotalCampaigns.ToString());
                parameter.Add("###SmsHitCount###", weeklyReportInfo.CampaignStatistics.SmsStatistics.TotalHits.ToString());
                parameter.Add("###SmsTotalReported###", weeklyReportInfo.CampaignStatistics.SmsStatistics.TotalReported.ToString());
                parameter.Add("###SmsProne%###", weeklyReportInfo.CampaignStatistics.SmsStatistics.PronePercentage.ToString());

                parameter.Add("###WhatsappTotalCount###", weeklyReportInfo.CampaignStatistics.WhatsappStatistics.TotalCampaigns.ToString());
                parameter.Add("###WhatsappHitCount###", weeklyReportInfo.CampaignStatistics.WhatsappStatistics.TotalHits.ToString());
                parameter.Add("###WhatsappTotalReported###", weeklyReportInfo.CampaignStatistics.WhatsappStatistics.TotalReported.ToString());
                parameter.Add("###WhatsappProne%###", weeklyReportInfo.CampaignStatistics.WhatsappStatistics.PronePercentage.ToString());

                parameter.Add("###TotalTrainingAssigned###", weeklyReportInfo.TrainingStatistics.Total.ToString());
                parameter.Add("###TrainingCompleted###", weeklyReportInfo.TrainingStatistics.Completed.ToString());
                parameter.Add("###TrainingInProgress###", weeklyReportInfo.TrainingStatistics.Incomplete.ToString());
                parameter.Add("###Completion%###", weeklyReportInfo.TrainingStatistics.CompletionPercentage.ToString());

                var EmailCampDepartmentLabel = weeklyReportInfo.CampaignStatistics.departmentHitReport.EmailCampaignDepartmentReport.Select(x => x.DepartmentName).ToArray(); // Convert to array
                var EmailCampDepartmentData = weeklyReportInfo.CampaignStatistics.departmentHitReport.EmailCampaignDepartmentReport.Select(x => x.PronePercentage).ToArray(); // Convert to array

                var SMSCampDepartmentLabel = weeklyReportInfo.CampaignStatistics.departmentHitReport.SmsCampaignDepartmentReport.Select(x => x.DepartmentName).ToArray(); // Convert to array
                var SMSCampDepartmentData = weeklyReportInfo.CampaignStatistics.departmentHitReport.SmsCampaignDepartmentReport.Select(x => x.PronePercentage).ToArray(); // Convert to array

                var WhastappCampDepartmentLabel = weeklyReportInfo.CampaignStatistics.departmentHitReport.WhatsappCampaignDepartmentReport.Select(x => x.DepartmentName).ToArray(); // Convert to array
                var WhastappCampDepartmentData = weeklyReportInfo.CampaignStatistics.departmentHitReport.WhatsappCampaignDepartmentReport.Select(x => x.PronePercentage).ToArray(); // Convert to array

                //var TrainingDepartmentLabel = weeklyReportInfo.TrainingStatistics.departmentWiseTrainingStatistics.Select(x => x.DepartmentName).ToArray(); // Convert to array
                //var TrainingDepartmentData = weeklyReportInfo.TrainingStatistics.departmentWiseTrainingStatistics.Select(x => x.Completed).ToArray(); // Convert to array

                parameter.Add("###EmailCampDepartmentLabel###", $"[{string.Join(", ", EmailCampDepartmentLabel.Select(d => $"'{d}'"))}]");
                parameter.Add("###EmailReportData###", $"[{string.Join(", ", EmailCampDepartmentData.Select(d => Convert.ToInt32(d)))}]");

                parameter.Add("###SMSCampDepartmentLabel###", $"[{string.Join(", ", SMSCampDepartmentLabel.Select(d => $"'{d}'"))}]");
                parameter.Add("###SMSReportData###", $"[{string.Join(", ", SMSCampDepartmentData.Select(d => Convert.ToInt32(d)))}]");


                parameter.Add("###WhatsappCampDepartmentLabel###", $"[{string.Join(", ", WhastappCampDepartmentLabel.Select(d => $"'{d}'"))}]");
                parameter.Add("###WhatsappReportData###", $"[{string.Join(", ", WhastappCampDepartmentData.Select(d => Convert.ToInt32(d)))}]");

                parameter.Add("###TrainingDepartmentLabel###", $"[]");
                parameter.Add("###TrainingReportData###", $"[]");

                //_weeklyReportTemplatePath = @"C:\Users\ChhaganSinha\Downloads\apex\WeeklyReportTemplate - Copy.html";
                var templateDirectory = Path.GetDirectoryName(_weeklyReportTemplatePath);
                if (!Directory.Exists(templateDirectory))
                    throw new Exception("Template directory does not exist: " + templateDirectory);

                if (!File.Exists(_weeklyReportTemplatePath))
                    throw new Exception("Template File doesn't exist");

                var content = File.ReadAllText(_weeklyReportTemplatePath);

                foreach (var item in parameter)
                {
                    content = content.Replace(item.Key, item.Value);
                }

                weeklyReportInfo.EmailDetails.Content = content;

                foreach (var o in observers)
                {
                    o.OnNext(weeklyReportInfo);
                }
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
