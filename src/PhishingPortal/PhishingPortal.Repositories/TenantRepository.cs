using Microsoft.Extensions.Logging;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto.Dashboard;
using Org.BouncyCastle.Asn1.X509;
using System.Linq;
using Humanizer;
using Org.BouncyCastle.Asn1.Mozilla;

namespace PhishingPortal.Repositories
{
    public class TenantRepository : BaseRepository, ITenantRepository, ISettingsRepository
    {
        public TenantRepository(ILogger logger, TenantDbContext dbContext)
            : base(logger)
        {
            TenantDbCtx = dbContext;
        }

        public virtual async Task<T> GetSetting<T>(string key)
        {
            T value = default(T);

            var v = await TenantDbCtx.Settings.FirstOrDefaultAsync(x => x.Key == key);

            if (v != null)
                value = (T)Convert.ChangeType(v.Value, typeof(T));

            return await Task.FromResult(value);
        }

        public Task<IEnumerable<Campaign>> GetAllCampaigns(int pageIndex, int pageSize)
        {
            var result = Enumerable.Empty<Campaign>();

            result = TenantDbCtx.Campaigns.Include(o => o.Detail).Include(o => o.Schedule).OrderByDescending(o => o.Id)
                .Skip(pageIndex * pageSize).Take(pageSize);

            return Task.FromResult(result);
        }


        public async Task<Campaign> GetCampaignById(int id)
        {
            Campaign result = null;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            result = TenantDbCtx.Campaigns.Include(o => o.Schedule).Include(o => o.Detail)
                .FirstOrDefault(o => o.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            return result;
        }
        public async Task<Campaign> GetCampaignByName(string name)
        {
            Campaign result = null;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            result = TenantDbCtx.Campaigns.Include(o => o.Schedule).Include(o => o.Detail)
                .FirstOrDefault(o => o.Name == name);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            return result;
        }

        public async Task<Campaign> UpsertCampaign(Campaign campaign)
        {
            Campaign result = null;
            var now = DateTime.Now;

            if (campaign == null)
                throw new ArgumentNullException("Invalid campaign data");

            campaign.IsActive = true;

            if (campaign.Id > 0)
            {
                campaign.ModifiedOn = now;
                campaign.Detail.ModifiedOn = now;

                if (campaign.Schedule.ScheduleType == ScheduleTypeEnum.NoSchedule)
                {
                    campaign.Schedule.ScheduleInfo = String.Empty;
                }

                TenantDbCtx.Campaigns.Update(campaign);

            }
            else
            {
                campaign.CreatedOn = now;
                campaign.Detail.CreatedOn = now;
                TenantDbCtx.Campaigns.Add(campaign);
            }
            try { TenantDbCtx.SaveChanges(); } 
            catch(Exception e) 
            {

            }
           
            return campaign;
        }

        public async Task<IEnumerable<CampaignTemplate>> GetAllTemplates(int pageIndex = 0, int pageSize = 10)
        {
            IEnumerable<CampaignTemplate> result = null;

            result = TenantDbCtx.CampaignTemplates.Skip(pageIndex * pageSize).Take(pageSize);

            return result;
        }


        public async Task<List<CampaignRecipient>> GetRecipientByCampaignId(int campaignId)
        {
            var result = TenantDbCtx.CampaignRecipients.Include(o => o.Recipient).Where(o => o.CampaignId == campaignId);

            return await Task.FromResult(result.ToList());
        }

        public async Task<List<RecipientImport>> ImportRecipientAsync(int campaignId, List<RecipientImport> data)
        {
            var hasChanges = false;
            foreach (var r in data)
            {
                if (!(TenantDbCtx.Recipients.Any(o => o.Email.Trim() == r.Email.Trim())))
                {
                    var recipient = new Recipient
                    {
                        Email = r.Email.Trim(),
                        Mobile = r.Mobile,
                        Name = r.Name,
                        WhatsAppNo = r.Mobile,
                        DateOfBirth = r.DateOfBirth,
                        Address = r.Address,
                        Branch = r.Branch,
                        Department = r.Department,
                        EmployeeCode = r.EmployeeCode,
                        IsActive = true
                    };

                    hasChanges = true;

                    TenantDbCtx.CampaignRecipients.Add(new CampaignRecipient
                    {
                        CampaignId = campaignId,
                        Recipient = recipient,
                        RecipientGroupId = null,
                    });

                }
                else
                {
                    var recipient = TenantDbCtx.Recipients
                        .FirstOrDefault(o => o.Email.Trim() == r.Email.Trim());

                    recipient.Mobile = r.Mobile;
                    recipient.Name = r.Name;
                    recipient.EmployeeCode = r.EmployeeCode;
                    recipient.Branch = r.Branch;
                    recipient.Department = r.Department;
                    recipient.DateOfBirth = r.DateOfBirth;


                    TenantDbCtx.Update(recipient);
                    hasChanges = true;
                    if (!(TenantDbCtx.CampaignRecipients.Any(o => o.CampaignId == campaignId && o.RecipientId == recipient.Id)))
                    {
                        TenantDbCtx.CampaignRecipients.Add(new CampaignRecipient
                        {
                            CampaignId = campaignId,
                            Recipient = recipient,
                            RecipientGroupId = null,
                        });
                    }
                }

            }

            if (hasChanges)
                TenantDbCtx.SaveChanges();

            else
            {
                throw new InvalidOperationException("No changes");
            }

            return await Task.FromResult(data);
        }

        public TenantDbContext TenantDbCtx { get; }

        public async Task<List<CampaignTemplate>> GetAllTemplates(CampaignType? type)
        {

            var templates = TenantDbCtx.CampaignTemplates.AsQueryable();

            if (type.HasValue)
            {
                templates = templates.Where(o => o.Type == type.Value);
            }

            return await templates.ToListAsync();
        }

        public async Task<CampaignTemplate> GetTemplateById(int id)
        {
            var template = TenantDbCtx.CampaignTemplates.FirstOrDefault(o => o.Id == id);
            return await Task.FromResult(template);
        }

        public async Task<CampaignTemplate> UpsertTemplate(CampaignTemplate template)
        {
            try
            {
                if (template.Id > 0)
                {
                    TenantDbCtx.Update(template);
                    TenantDbCtx.SaveChanges();
                }
                else
                {
                    TenantDbCtx.Add(template);
                    TenantDbCtx.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return template;
        }

        public async Task<Tuple<bool, string>> CampaignHit(string key)
        {
            var status = CampaignLogStatus.Sent.ToString();
            var campaignLog = TenantDbCtx.CampaignLogs
                .FirstOrDefault(o => o.SecurityStamp == key
                                    && o.IsHit == false && o.Status == status);

            if (campaignLog == null)
                throw new Exception("Invalid Url");

            var campaign = TenantDbCtx.Campaigns.FirstOrDefault(o => o.Id == campaignLog.CampaignId);
            if (campaign == null)
                throw new Exception("Invalid Campaign");

            campaignLog.Status = CampaignLogStatus.Completed.ToString();
            campaignLog.IsHit = true;
            campaignLog.ModifiedOn = DateTime.Now;
            campaignLog.ModifiedBy = nameof(CampaignHit);

            TenantDbCtx.Update(campaignLog);
            TenantDbCtx.SaveChanges();

            return await Task.FromResult(new Tuple<bool, string>(true, campaign.ReturnUrl));

        }


        /// <summary
        /// Phsihing prone percentage - group by phishing category
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>


        public async Task<CategoryWisePhishingTestData> GetCategoryWisePhishingReport(DateTime start, DateTime end)
        {
            CategoryWisePhishingTestData data = new CategoryWisePhishingTestData();

            data.Entries = new List<PhisingPronePercentEntry>();
            data.SmsEntries = new List<PhisingPronePercentEntry>();
            data.WhatsappEntries = new List<PhisingPronePercentEntry>();

            data.CategoryClickRatioDictionary = new Dictionary<string, decimal>();
            data.SmsCategoryClickRatioDictionary = new Dictionary<string, decimal>();
            data.WhatsappCategoryClickRatioDictionary = new Dictionary<string, decimal>();

            data.DepartEntries = new Dictionary<string, decimal>();
            data.SmsDepartEntries = new Dictionary<string, decimal>();
            data.WhatsappDepartEntries = new Dictionary<string, decimal>();

            data.TemplateClickEntries = new Dictionary<string, decimal>();
            data.SmsTemplateClickEntries = new Dictionary<string, decimal>();
            data.WhatsappTemplateClickEntries = new Dictionary<string, decimal>();

            try
            {

                var totatPhishingTests = TenantDbCtx.CampaignLogs
                  .Where(i => i.CreatedOn >= start && i.CreatedOn < end);

                #region EMAIL CHART ANALYSIS
                var campaignGroup = totatPhishingTests.Where(o => o.CampignType == CampaignType.Email.ToString()).ToList().GroupBy(i => i.CampaignId, (key, entries) => new
                {
                    CampaignId = key,
                    Total = entries.Count(),
                    TotalHits = entries.Count(i => i.IsHit),
                    TotalReported = entries.Count(i => i.IsReported),
                });

                foreach (var c in campaignGroup)
                {
                    var campaign = TenantDbCtx.Campaigns.Find(c.CampaignId);

                    if (campaign == null)
                        continue;

                    var entry = new PhisingPronePercentEntry()
                    {
                        Campaign = campaign,
                        Count = c.Total,
                        Hits = c.TotalHits,
                        Reported = c.TotalReported,
                    };

                    data.Entries.Add(entry);
                }
                data.Entries = data.Entries.OrderByDescending(o => o.Campaign.ModifiedOn).Take(5).ToList();

                data.TotalCampaigns = data.Entries.Sum(i => i.Count);
                var ids = data.Entries;

                //email campaign - department wise data 

                int count = 0, id1 = 0, id2 = 0, id3 = 0, id4 = 0, id5 = 0;
                foreach (var id in ids)
                {
                    count += 1;
                    if (count == 1)
                        id1 = id.Campaign.Id;
                    if (count == 2)
                        id2 = id.Campaign.Id;
                    if (count == 3)
                        id3 = id.Campaign.Id;
                    if (count == 4)
                        id4 = id.Campaign.Id;
                    if (count == 5)
                        id5 = id.Campaign.Id;

                }

                var filterData = totatPhishingTests.Where(o => o.CampaignId == id1 || o.CampaignId == id2 || o.CampaignId == id3 || o.CampaignId == id4 || o.CampaignId == id5);

                var phishtestWithRecipients = from log in filterData
                                              join crec in TenantDbCtx.CampaignRecipients.Include(o => o.Recipient) on log.RecipientId equals crec.RecipientId
                                              select new { logEntry = log, Department = crec.Recipient.Department };

                var depatwiseCnt = phishtestWithRecipients.Where(a => a.logEntry.CampignType == CampaignType.Email.ToString()).ToList().GroupBy(i => i.Department, (key, entries) => new
                {
                    Department = key ?? "UNKNOWN",
                    Total = entries.Count(),
                    Hits = entries.Count(o => o.logEntry.IsHit),
                    Reported = entries.Count(o => o.logEntry.IsReported),
                });
                var DtotalHits = depatwiseCnt.Sum(o => o.Hits);

                foreach (var dep in depatwiseCnt)
                {
                    // var depart = data.Entries.Where(o=>o);
                    if (dep.Total > 0)
                    {
                        var equivDep = CalcEquivalentPercent(dep.Hits, DtotalHits);

                        if (!data.DepartEntries.ContainsKey(dep.Department))
                        {
                            data.DepartEntries.Add(dep.Department, equivDep);
                        }


                    }
                }
                // Percentage of Email Campaign Template report
                var phishtestWithTemp = from log in totatPhishingTests
                                        join cdet in TenantDbCtx.CampaignDetails on log.CampaignId equals cdet.CampaignId
                                        join ctem in TenantDbCtx.CampaignTemplates on cdet.CampaignTemplateId equals ctem.Id
                                        select new { logEntry = log, template = ctem.Name, templateId = ctem.Id };

                var tempwiseCnt = phishtestWithTemp.Where(a => a.logEntry.CampignType == CampaignType.Email.ToString()).ToList().GroupBy(i => i.templateId + "." + i.template, (key, tentries) => new
                {
                    template = key,

                    TTotal = tentries.Count(),
                    THits = tentries.Count(o => o.logEntry.IsHit),

                });
                var TemptotalHits = tempwiseCnt.Sum(o => o.THits);

                foreach (var tem in tempwiseCnt)
                {
                    if (tem.TTotal > 0)
                    {
                        var equivTemp = CalcEquivalentPercent(tem.THits, TemptotalHits);

                        if (!data.TemplateClickEntries.ContainsKey(tem.template))
                        {
                            data.TemplateClickEntries.Add(tem.template, equivTemp);
                        }
                    }
                }

                // create category wise phish prone %
                var categoryWiseGrp = data.Entries.GroupBy(o => o.Campaign.Category, (key, values) => new
                {
                    Category = key,
                    Count = values.Sum(o => o.Count),
                    HitCount = values.Sum(o => o.Hits),
                    ReportedCount = values.Sum(o => o.Reported),
                });

                var totalHits = categoryWiseGrp.Sum(o => o.HitCount);

                // calc phishing percentage out of total hits
                foreach (var category in categoryWiseGrp)
                {
                    if (category.Count > 0 && data.TotalCampaigns > 0)
                    {
                        var equivalenPp = CalcEquivalentPercent(category.HitCount, totalHits);

                        if (!data.CategoryClickRatioDictionary.ContainsKey(category.Category))
                        {
                            data.CategoryClickRatioDictionary.Add(category.Category, equivalenPp);
                        }


                    }
                }
                #endregion

                #region SMS CHART ANALYSIS
                var SmsCampaignGroup = totatPhishingTests.Where(o => o.CampignType == CampaignType.Sms.ToString()).ToList().GroupBy(i => i.CampaignId, (key, entries) => new
                {
                    CampaignId = key,
                    Total = entries.Count(),
                    TotalHits = entries.Count(i => i.IsHit),
                    TotalReported = entries.Count(i => i.IsReported),
                });

                foreach (var c in SmsCampaignGroup)
                {
                    var campaign = TenantDbCtx.Campaigns.Find(c.CampaignId);

                    if (campaign == null)
                        continue;

                    var entry = new PhisingPronePercentEntry()
                    {
                        Campaign = campaign,
                        Count = c.Total,
                        Hits = c.TotalHits,
                        Reported = c.TotalReported,
                    };

                    data.SmsEntries.Add(entry);
                }
                data.SmsEntries = data.SmsEntries.OrderByDescending(o => o.Campaign.ModifiedOn).Take(5).ToList();
                data.TotalSmsCampaigns = data.SmsEntries.Sum(i => i.Count);
                var sIds = data.SmsEntries;
                int sCount = 0, sId1 = 0, sId2 = 0, sId3 = 0, sId4 = 0, sId5 = 0;
                foreach (var id in sIds)
                {
                    sCount += 1;
                    if (sCount == 1)
                        sId1 = id.Campaign.Id;
                    if (sCount == 2)
                        sId2 = id.Campaign.Id;
                    if (sCount == 3)
                        sId3 = id.Campaign.Id;
                    if (sCount == 4)
                        sId4 = id.Campaign.Id;
                    if (sCount == 5)
                        sId5 = id.Campaign.Id;

                }

                var filterSmsData = totatPhishingTests.Where(o => o.CampaignId == sId1 || o.CampaignId == sId2 || o.CampaignId == sId3 || o.CampaignId == sId4 || o.CampaignId == sId5);

                var smsPhishtestWithRecipients = from log in filterSmsData
                                                      join crec in TenantDbCtx.CampaignRecipients.Include(o => o.Recipient) on log.RecipientId equals crec.RecipientId
                                                      select new { logEntry = log, Department = crec.Recipient.Department };
                //**************************Department for Sms camp**************************************************

                var SmsDepatwiseCount = smsPhishtestWithRecipients.Where(a => a.logEntry.CampignType == CampaignType.Sms.ToString()).ToList().GroupBy(i => i.Department, (key, entries) => new
                {
                    Department = key ?? "UNKNOWN",
                    Total = entries.Count(),
                    Hits = entries.Count(o => o.logEntry.IsHit),
                    Reported = entries.Count(o => o.logEntry.IsReported),
                });
                var SmsDtotalHits = SmsDepatwiseCount.Sum(o => o.Hits);

                foreach (var dep in SmsDepatwiseCount)
                {
                    if (dep.Total > 0)
                    {
                        var equivDep = CalcEquivalentPercent(dep.Hits, SmsDtotalHits);

                        if (!data.SmsDepartEntries.ContainsKey(dep.Department))
                        {
                            data.SmsDepartEntries.Add(dep.Department, equivDep);
                        }


                    }
                }
                //*************************Sms Template wise**********************************
                var SmstempwiseCnt = phishtestWithTemp.Where(a => a.logEntry.CampignType == CampaignType.Sms.ToString()).ToList().GroupBy(i => i.templateId + "." + i.template, (key, tentries) => new
                {
                    template = key,
                    TTotal = tentries.Count(),
                    THits = tentries.Count(o => o.logEntry.IsHit)
                });
                var SmsTemptotalHits = SmstempwiseCnt.Sum(o => o.THits);

                foreach (var tem in SmstempwiseCnt)
                {
                    if (tem.TTotal > 0)
                    {
                        var equivTemp = CalcEquivalentPercent(tem.THits, SmsTemptotalHits);

                        if (!data.SmsTemplateClickEntries.ContainsKey(tem.template))
                        {
                            data.SmsTemplateClickEntries.Add(tem.template, equivTemp);
                        }
                    }
                }
                //*************************** Sms category wise********************************************
                var SmsCategoryWiseGrp = data.SmsEntries.GroupBy(o => o.Campaign.Category, (key, values) => new
                {
                    Category = key,
                    Count = values.Sum(o => o.Count),
                    HitCount = values.Sum(o => o.Hits),
                    ReportedCount = values.Sum(o => o.Reported),
                });

                var SmstotalHits = SmsCategoryWiseGrp.Sum(o => o.HitCount);

                // calc phishing percentage out of total hits
                foreach (var category in SmsCategoryWiseGrp)
                {
                    if (category.Count > 0 && data.TotalSmsCampaigns > 0)
                    {
                        var equivalenPp = CalcEquivalentPercent(category.HitCount, SmstotalHits);

                        if (!data.SmsCategoryClickRatioDictionary.ContainsKey(category.Category))
                        {
                            data.SmsCategoryClickRatioDictionary.Add(category.Category, equivalenPp);
                        }


                    }
                }
                #endregion

                #region WHATSAPP CHART ANALYSIS
                var WhatsappcampaignGroup = totatPhishingTests.Where(o => o.CampignType == CampaignType.Whatsapp.ToString()).ToList().GroupBy(i => i.CampaignId, (key, entries) => new
                {
                    CampaignId = key,
                    Total = entries.Count(),
                    TotalHits = entries.Count(i => i.IsHit),
                    TotalReported = entries.Count(i => i.IsReported),
                });

                foreach (var c in WhatsappcampaignGroup)
                {
                    var campaign = TenantDbCtx.Campaigns.Find(c.CampaignId);

                    if (campaign == null)
                        continue;

                    var entry = new PhisingPronePercentEntry()
                    {
                        Campaign = campaign,
                        Count = c.Total,
                        Hits = c.TotalHits,
                        Reported = c.TotalReported,
                    };

                    data.WhatsappEntries.Add(entry);
                }
                data.WhatsappEntries = data.WhatsappEntries.OrderByDescending(o => o.Campaign.ModifiedOn).Take(5).ToList();
                data.TotalWhatsappCampaigns = data.WhatsappEntries.Sum(i => i.Count);
                var WhatsappIds = data.WhatsappEntries;
                int wCount = 0, wid1 = 0, wid2 = 0, wid3 = 0, wid4 = 0, wid5 = 0;
                foreach (var id in WhatsappIds)
                {
                    wCount += 1;
                    if (wCount == 1)
                        wid1 = id.Campaign.Id;
                    if (wCount == 2)
                        wid2 = id.Campaign.Id;
                    if (wCount == 3)
                        wid3 = id.Campaign.Id;
                    if (wCount == 4)
                        wid4 = id.Campaign.Id;
                    if (wCount == 5)
                        wid5 = id.Campaign.Id;

                }

                var filterWhatsappData = totatPhishingTests.Where(o => o.CampaignId == wid1 || o.CampaignId == wid2 || o.CampaignId == wid3 || o.CampaignId == wid4 || o.CampaignId == wid5);

                var whatsappPhishtestWithRecipients = from log in filterWhatsappData
                                                      join crec in TenantDbCtx.CampaignRecipients.Include(o => o.Recipient) on log.RecipientId equals crec.RecipientId
                                              select new { logEntry = log, Department = crec.Recipient.Department };
                //**************************Department for Whatsapp camp**************************************************
                var WhatsappDepatwiseCount = whatsappPhishtestWithRecipients.Where(a => a.logEntry.CampignType == CampaignType.Whatsapp.ToString()).ToList().GroupBy(i => i.Department, (key, entries) => new
                {
                    Department = key ?? "UNKNOWN",
                    Total = entries.Count(),
                    Hits = entries.Count(o => o.logEntry.IsHit),
                    Reported = entries.Count(o => o.logEntry.IsReported),
                });
                var WhatsappDtotalHits = WhatsappDepatwiseCount.Sum(o => o.Hits);

                foreach (var dep in WhatsappDepatwiseCount)
                {
                    if (dep.Total > 0)
                    {
                        var equivDep = CalcEquivalentPercent(dep.Hits, WhatsappDtotalHits);

                        if (!data.WhatsappDepartEntries.ContainsKey(dep.Department))
                        {
                            data.WhatsappDepartEntries.Add(dep.Department, equivDep);
                        }


                    }
                }
                //*************************Whatsapp Template wise**********************************

                var WhatsappTempwiseCnt = phishtestWithTemp.Where(a => a.logEntry.CampignType == CampaignType.Whatsapp.ToString()).ToList().GroupBy(i => i.templateId + "." + i.template, (key, tentries) => new
                {
                    template = key,
                    TTotal = tentries.Count(),
                    THits = tentries.Count(o => o.logEntry.IsHit)
                });
                var WhatsappTemptotalHits = WhatsappTempwiseCnt.Sum(o => o.THits);

                foreach (var tem in WhatsappTempwiseCnt)
                {
                    if (tem.TTotal > 0)
                    {
                        var equivTemp = CalcEquivalentPercent(tem.THits, WhatsappTemptotalHits);

                        if (!data.WhatsappTemplateClickEntries.ContainsKey(tem.template))
                        {
                            data.WhatsappTemplateClickEntries.Add(tem.template, equivTemp);
                        }
                    }
                }

                //*************************** Whatsapp category wise********************************************
                var WhatsappCategoryWiseGrp = data.WhatsappEntries.GroupBy(o => o.Campaign.Category, (key, values) => new
                {
                    Category = key,
                    Count = values.Sum(o => o.Count),
                    HitCount = values.Sum(o => o.Hits),
                    ReportedCount = values.Sum(o => o.Reported),
                });

                var WhatsapptotalHits = WhatsappCategoryWiseGrp.Sum(o => o.HitCount);

                // calc phishing percentage out of total hits
                foreach (var category in WhatsappCategoryWiseGrp)
                {
                    if (category.Count > 0 && data.TotalWhatsappCampaigns > 0)
                    {
                        var equivalenPp = CalcEquivalentPercent(category.HitCount, WhatsapptotalHits);

                        if (!data.WhatsappCategoryClickRatioDictionary.ContainsKey(category.Category))
                        {
                            data.WhatsappCategoryClickRatioDictionary.Add(category.Category, equivalenPp);
                        }


                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }

            return await Task.FromResult(data);
        }

        private decimal CalcEquivalentPercent(int hitCount, int totalHits)
        {
            decimal hitPercent = 0.0M;

            if (totalHits == 0)
                hitPercent = 0;
            else
                hitPercent = ((decimal)hitCount / totalHits) * 100;

            return hitPercent;

        }

        /// <summary>
        /// Get stats for each type of campaign last executed
        /// </summary>
        /// <returns></returns>
        public async Task<ConsolidatedPhishingStats> GetLastPhishingStatics()
        {
            var outcome = new ConsolidatedPhishingStats();

            // emails
            var lastEmailCmpgn = TenantDbCtx.Campaigns.Include(o => o.Detail)
                .Where(o => o.State == CampaignStateEnum.Completed && o.Detail.Type == CampaignType.Email)
                .OrderByDescending(c => c.ModifiedOn)
                .FirstOrDefault();

            if (lastEmailCmpgn != null)
            {
                var logs = TenantDbCtx.CampaignLogs.Where(o => o.CampaignId == lastEmailCmpgn.Id
                             && (o.Status == CampaignLogStatus.Sent.ToString() || o.Status == CampaignLogStatus.Completed.ToString()));

                outcome.Email.Total = logs.Count();
                outcome.Email.TotalHits = logs.Count(o => o.IsHit);
                outcome.Email.TotalReported = logs.Count(o => o.IsReported);
                if (outcome.Email.Total > 0)
                {
                    outcome.Email.PronePercent = Math.Round(((decimal)outcome.Email.TotalHits / outcome.Email.Total) * 100, 2);
                }
            }

            // sms
            var lastSmsCmgn = TenantDbCtx.Campaigns.Include(o => o.Detail)
                .Where(o => o.State == CampaignStateEnum.Completed && o.Detail.Type == CampaignType.Sms)
                .OrderByDescending(c => c.ModifiedOn)
                .FirstOrDefault();

            if (lastSmsCmgn != null)
            {
                var logs = TenantDbCtx.CampaignLogs.Where(o => o.CampaignId == lastSmsCmgn.Id
                             && (o.Status == CampaignLogStatus.Sent.ToString() || o.Status == CampaignLogStatus.Completed.ToString()));

                outcome.Sms.Total = logs.Count();
                outcome.Sms.TotalHits = logs.Count(o => o.IsHit);
                outcome.Sms.TotalReported = logs.Count(o => o.IsReported);
                if (outcome.Sms.Total > 0)
                {
                    outcome.Sms.PronePercent = Math.Round(((decimal)outcome.Sms.TotalHits / outcome.Sms.Total) * 100, 2);
                }
            }

            //whatsapp
            var lastWaCmgn = TenantDbCtx.Campaigns.Include(o => o.Detail)
                .Where(o => o.State == CampaignStateEnum.Completed && o.Detail.Type == CampaignType.Whatsapp)
                .OrderByDescending(c => c.ModifiedOn)
                .FirstOrDefault();

            if (lastWaCmgn != null)
            {
                var logs = TenantDbCtx.CampaignLogs.Where(o => o.CampaignId == lastWaCmgn.Id
                             && (o.Status == CampaignLogStatus.Sent.ToString() || o.Status == CampaignLogStatus.Completed.ToString()));

                outcome.Whatsapp.Total = logs.Count();
                outcome.Whatsapp.TotalHits = logs.Count(o => o.IsHit);
                outcome.Whatsapp.TotalReported = logs.Count(o => o.IsReported);
                if (outcome.Whatsapp.Total > 0)
                {
                    outcome.Whatsapp.PronePercent = Math.Round(((decimal)outcome.Whatsapp.TotalHits / outcome.Whatsapp.Total) * 100, 2);
                }
            }

            return await Task.FromResult(outcome);
        }

        /// <summary>
        /// Month wise phishing tests report for the year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<MonthlyPhishingBarChart> GetMonthlyBarChart(int year)
        {
            MonthlyPhishingBarChart data = new MonthlyPhishingBarChart();
            data.Title = "Monthly Phishing Tests Statistics";
            data.Year = year;
            data.Entries = new List<MonthlyPhishingBarChartEntry>();

            var yearStartDt = new DateTime(year, 1, 1);

            var yearEndDt = new DateTime(year, 12, 31).AddHours(24).AddSeconds(-1);

            var totatPhishingTests = TenantDbCtx.CampaignLogs
                .Where(i => i.CreatedOn >= yearStartDt && i.CreatedOn < yearEndDt);

            var monthlyGroup = totatPhishingTests.ToList().GroupBy(i => i.CreatedOn.Month, (key, entries) => new
            {
                Month = (Months)key,
                Total = entries.Count(),
                TotalHits = entries.Count(i => i.IsHit),
                Percent = 0.0f,
            });


            foreach (Months month in Enum.GetValues(typeof(Months)))
            {

                var log = monthlyGroup.FirstOrDefault(i => i.Month == month);

                var entry = new MonthlyPhishingBarChartEntry
                {
                    Month = month,
                };
#if DEBUG

                // this is just to test dashboard
                //if (month <= (Months)DateTime.Now.Month)
                //{
                //    entry.TotalCount = new Random().Next(50, 100);
                //    entry.TotalHits = new Random().Next(15, 39);
                //    entry.HitPronePercent = Math.Round(((decimal)entry.TotalHits / (decimal)entry.TotalCount) * 100, 2);
                //}
                if (log != null)
                {
                    entry.TotalCount = log.Total;
                    entry.TotalHits = log.TotalHits;
                    if (log.Total > 0)
                        entry.HitPronePercent = (log.TotalHits / log.Total) * 100;

                }

#else
                if (log != null)
                {
                    entry.TotalCount = log.Total;
                    entry.TotalHits = log.TotalHits;
                    if (log.Total > 0)
                        entry.HitPronePercent = (log.TotalHits / log.Total) * 100;

                }
#endif

                data.Entries.Add(entry);
            }
            return await Task.FromResult(data);
        }

        /// <summary>
        /// GetRecipientGroups
        /// </summary>
        /// <param name="adGroupOnly"></param>
        /// <returns></returns>
        public async Task<List<RecipientGroup>> GetRecipientGroups(bool adGroupOnly = false)
        {
            var query = TenantDbCtx.RecipientGroups.AsQueryable();

            if (adGroupOnly)
                query = query.Where(o => o.IsActiveDirectoryGroup);

            return await query.ToListAsync();
        }

        /// <summary>
        /// GetRecipientsByGroupId
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<List<Recipient>> GetRecipientsByGroupId(int groupId)
        {
            var query = from rm in TenantDbCtx.RecipientGroupMappings
                        join r in TenantDbCtx.Recipients on rm.RecipientId equals r.Id
                        where rm.GroupId == groupId
                        select r;

            return await query.ToListAsync();
        }

        /// <summary>
        /// InsertRecipientGroupMappings
        /// </summary>
        /// <param name="group"></param>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public async Task<List<Recipient>> ImportAdGroupMembers(RecipientGroup group, List<Recipient> recipients)
        {
            try
            {
                var grp = TenantDbCtx.RecipientGroups.FirstOrDefault(o => o.Uid == group.Uid);

                if (grp == null)
                {
                    group.LastImported = DateTime.Now;
                    grp = group;
                    TenantDbCtx.RecipientGroups.Add(group);
                    TenantDbCtx.SaveChanges();
                }

                var recipientsToGroup = new List<Recipient>();
                // add recipient if not existed
                foreach (var recipient in recipients)
                {
                    var r = TenantDbCtx.Recipients.FirstOrDefault(o => o.Email == recipient.Email);

                    if (r == null)
                    {
                        r = recipient;
                        TenantDbCtx.Add(recipient);
                    }
                    recipientsToGroup.Add(r);
                }
                TenantDbCtx.SaveChanges();

                // map them
                foreach (var recipient in recipientsToGroup)
                {
                    var rm = TenantDbCtx.RecipientGroupMappings.AsNoTracking()
                        .FirstOrDefault(o => o.GroupId == grp.Id && o.RecipientId == recipient.Id);

                    if (rm == null)
                    {
                        TenantDbCtx.Add(new RecipientGroupMapping { GroupId = grp.Id, RecipientId = recipient.Id });

                    }
                }

                TenantDbCtx.SaveChanges();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                return Enumerable.Empty<Recipient>().ToList();
            }

            return await Task.FromResult(recipients);
        }

        #region Settings
        public async Task<Dictionary<string, string>> GetSettings()
        {
            var settings = await TenantDbCtx.Settings.Where(o => o.Key != TenantData.Keys.License && o.Key != TenantData.Keys.PublicKey).ToListAsync();

            return settings.ToDictionary(o => o.Key, v => v.Value);
        }

        public (string? LicenseKey, string? PublicKey) GetLicenseKeys() {

            var publicKey = TenantDbCtx.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.PublicKey);
            var licenseKey = TenantDbCtx.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.License);
            return (licenseKey?.Value, publicKey?.Value);
        }

        public async Task<Dictionary<string, string>> UpsertSettings(Dictionary<string, string> settings, string user)
        {
            var inserted = new Dictionary<string, string>();

            try
            {
                foreach (var key in settings.Keys)
                {
                    var setting = await TenantDbCtx.Settings.FirstOrDefaultAsync(o => o.Key == key);

                    if (setting != null)
                    {
                        setting.Value = settings[key];
                        setting.ModifiedOn = DateTime.Now;
                        setting.ModifiedBy = user;
                        TenantDbCtx.Update(setting);
                    }
                    else
                    {
                        setting = new TenantSetting()
                        {
                            Key = key,
                            Value = settings[key],
                            CreatedBy = user,
                            CreatedOn = DateTime.Now,
                        };
                        TenantDbCtx.Add(setting);
                    }

                    inserted.Add(key, setting.Value);

                    TenantDbCtx.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
            return inserted;
        }
        #endregion

        public async Task<Training> UpsertTraining(Training training)
        {
            try
            {

                if (training.Id > 0)
                {
                    if (training.TrainingSchedule.ScheduleType == ScheduleTypeEnum.NoSchedule)
                    {
                        training.TrainingSchedule.ScheduleInfo = String.Empty;
                    }
                    TenantDbCtx.Update(training);
                    TenantDbCtx.SaveChanges();
                }
                else
                {
                    TenantDbCtx.Add(training);
                    TenantDbCtx.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return training;
        }
        public async Task<Training> GetTrainingById(int id)
        {
            Training result = null;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            result = TenantDbCtx.Training.Include(o => o.TrainingSchedule).FirstOrDefault(o => o.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            return result;
        }


        public async Task<List<RecipientImport>> ImportTrainingRecipient(int trainingId, List<RecipientImport> data)
        {
            var hasChanges = false;
            foreach (var r in data)
            {
                if (!TenantDbCtx.Recipients.Any(o => o.Email == r.Email || o.Mobile == r.Mobile))
                {
                    var recipient = new Recipient
                    {
                        Email = r.Email,
                        Mobile = r.Mobile,
                        Name = r.Name,
                        WhatsAppNo = r.Mobile,
                        DateOfBirth = r.DateOfBirth,
                        Address = r.Address,
                        Branch = r.Branch,
                        Department = r.Department,
                        EmployeeCode = r.EmployeeCode,
                        IsActive = true
                    };

                    hasChanges = true;

                    TenantDbCtx.TrainingRecipient.Add(new TrainingRecipients
                    {
                        TrainingId = trainingId,
                        AllTrainingRecipient = recipient,
                        RecipientGroupId = null,
                    });

                }
                else
                {
                    var recipient = TenantDbCtx.Recipients
                        .FirstOrDefault(o => o.Email == r.Email || o.Mobile == r.Mobile);

                    recipient.Mobile = r.Mobile;
                    recipient.Name = r.Name;
                    recipient.Email = r.Email;

                    TenantDbCtx.Update(recipient);
                    hasChanges = true;
                    if (!TenantDbCtx.TrainingRecipient.Any(o => o.TrainingId == trainingId && o.RecipientId == recipient.Id))
                    {
                        TenantDbCtx.TrainingRecipient.Add(new TrainingRecipients
                        {
                            TrainingId = trainingId,
                            AllTrainingRecipient = recipient,
                            RecipientGroupId = null,
                        });
                    }
                }

            }

            if (hasChanges)
                TenantDbCtx.SaveChanges();
            else
            {
                throw new InvalidOperationException("No changes");
            }

            return await Task.FromResult(data);
        }

        public async Task<List<TrainingRecipients>> GetRecipientByTrainingId(int trainingId)
        {
            var result = TenantDbCtx.TrainingRecipient.Include(o => o.AllTrainingRecipient).Where(o => o.TrainingId == trainingId);

            return await Task.FromResult(result.ToList());
        }

        public async Task<Tuple<bool, string>> Training(string key)
        {
            var status = TrainingLogStatus.Sent.ToString();
            var trainingLog = await TenantDbCtx.TrainingLog.FirstOrDefaultAsync(o => o.SecurityStamp == key && o.Status == status);

            if (trainingLog == null)
                throw new Exception("Invalid log");

            var training = await TenantDbCtx.Training.FirstOrDefaultAsync(o => o.Id == trainingLog.TrainingID);
            if (training == null)
                throw new Exception("Invalid Training");

            trainingLog.Status = TrainingState.InProgress.ToString();

            TenantDbCtx.Update(trainingLog);
            await TenantDbCtx.SaveChangesAsync();

            return new Tuple<bool, string>(true, training.TrainingCategory);
        }

        public async Task<MonthlyTrainingBarChart> GetTrainingReportData(int year)
        {
            MonthlyTrainingBarChart data = new MonthlyTrainingBarChart();
            data.MonthwiseTrainingEntry = new List<MonthlyTrainingReportData>();
            data.Year = year;
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31).AddHours(24).AddSeconds(-1);

            try
            {
                var Traininglogs = TenantDbCtx.TrainingLog
                 .Where(i => i.SentOn >= start && i.SentOn < end);

                #region
                //var trainingGroup = Trainings.ToList().GroupBy(i => i.TrainingID, (key, entries) => new
                //{
                //    TrainingID = key,
                //    TotalTraining = entries.Count(),
                //    Completed = entries.Where(i => i.Equals(TrainingStatus.Completed)).Count(),
                //    Inprogress = entries.Where(i => i.Equals(TrainingStatus.InProgress)).Count()
                //});
                //foreach (var c in trainingGroup)
                //{
                //    var training = TenantDbCtx.Training.Find(c.TrainingID);

                //    if (training == null)
                //        continue;

                //    var entry = new TrainingCountsEntry()
                //    {
                //        Training = training,
                //        TotalTrainingAssign = c.TotalTraining,
                //        TrainingCompleted = c.Completed,
                //        TrainingInprogess = c.Inprogress,
                //    };

                //    TRData.TrainingCountEntries.Add(entry);
                //}
                //TRData.TrainingCountEntries = TRData.TrainingCountEntries.OrderByDescending(o => o.Training.ModifiedOn).Take(5).ToList();
                #endregion


                var trainingGroup = Traininglogs.ToList().GroupBy(i => i.SentOn.Month, (key, entries) => new
                {
                    Month = (Months)key,
                    TotalTraining = entries.Count(),
                    Completed = entries.Count(i => i.Status == (TrainingLogStatus.Completed).ToString()),
                    Inprogress = entries.Count(i => i.Status == (TrainingLogStatus.InProgress).ToString()),

                });


                foreach (Months month in Enum.GetValues(typeof(Months)))
                {

                    var log = trainingGroup.FirstOrDefault(i => i.Month == month);

                    var entry = new MonthlyTrainingReportData
                    {
                        Month = month,
                    };

                    if (log != null)
                    {
                        entry.TotalTraining = log.TotalTraining;
                        entry.Completed = log.Completed;
                        entry.Inprogress = log.Inprogress;
                        if (entry.TotalTraining > 0)
                        {
                            entry.CompletionPercent = Math.Round(((decimal)entry.Completed / entry.TotalTraining) * 100, 2);
                        }

                    }


                    data.MonthwiseTrainingEntry.Add(entry);
                }




            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }
            return await Task.FromResult(data);
        }

        public async Task<TrainingStatics> GetLastTrainingStatics()
        {
            var outcome = new TrainingStatics();

            var lastTraining = TenantDbCtx.Training.Where(o => o.State == TrainingState.Completed)
                .OrderByDescending(c => c.ModifiedOn)
                .FirstOrDefault();

            if (lastTraining != null)
            {
                var logs = TenantDbCtx.TrainingLog.Where(o => o.TrainingID == lastTraining.Id
                             && (o.Status == TrainingLogStatus.Sent.ToString() || o.Status == TrainingLogStatus.Completed.ToString()));

                outcome.TotalTrainingAssign = logs.Count();
                outcome.TrainingCompleted = logs.Count(o => o.Status == TrainingLogStatus.Completed.ToString());
                outcome.TrainingInprogess = logs.Count(o => o.Status == TrainingLogStatus.InProgress.ToString());
                outcome.TrainingNotAttampt = logs.Count(o => o.Status == TrainingLogStatus.Sent.ToString());

                if (outcome.TotalTrainingAssign > 0)
                {
                    outcome.TrainingCompromised = Math.Round(((decimal)outcome.TrainingNotAttampt / outcome.TotalTrainingAssign) * 100, 2);
                }
            }


            return await Task.FromResult(outcome);
        }

        public async Task<List<int>> GetYearList()
        {
            var years = TenantDbCtx.CampaignLogs.Select(c => c.CreatedOn.Year).Distinct().ToList();
            return await Task.FromResult(years);
        }

        public async Task<List<Campaign>> GetCampaignsName()
        {
            int currentYear = DateTime.Now.Year;
            int previousYear = currentYear - 1;
            var result = Enumerable.Empty<Campaign>();
            //result = TenantDbCtx.Campaigns.Where(o => o.CreatedOn.Year == currentYear || o.CreatedOn.Year == previousYear).OrderByDescending(o => o.Id);
            result = TenantDbCtx.Campaigns.OrderByDescending(o => o.Id).ToList();
            return (List<Campaign>)result;
        }

        public async Task<bool> UpsertTrainingCampaignMap(Dictionary<int, List<int>> dict)
        {
            foreach (var kvp in dict)
            {
                var trainingId = kvp.Key;
                var campaignIds = kvp.Value;

                var existingCampaignIds = await TenantDbCtx.TrainingCampaignMapping
                    .Where(tcm => tcm.TrainingId == trainingId)
                    .Select(tcm => tcm.CampaignId)
                    .ToListAsync();

                var campaignIdsToAdd = campaignIds.Except(existingCampaignIds);
                var campaignIdsToRemove = existingCampaignIds.Except(campaignIds);
                var campaignIdsToKeep = campaignIds.Intersect(existingCampaignIds);

                if (campaignIdsToRemove.Any())
                {
                    var mappingsToRemove = await TenantDbCtx.TrainingCampaignMapping
                        .Where(tcm => tcm.TrainingId == trainingId && campaignIdsToRemove.Contains(tcm.CampaignId))
                        .ToListAsync();
                    TenantDbCtx.RemoveRange(mappingsToRemove);
                }

                if (campaignIdsToAdd.Any())
                {
                    var mappingsToAdd = campaignIdsToAdd.Select(c => new TrainingCampaignMapping
                    {
                        TrainingId = trainingId,
                        CampaignId = c
                    });
                    TenantDbCtx.AddRange(mappingsToAdd);
                }

                await TenantDbCtx.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpsertCampaignTrainingMap(CampaignTrainingIdcs campaignTrainingIdcs)
        {
            var trainingId = campaignTrainingIdcs.TrainingId;
            var campaignId = campaignTrainingIdcs.CampaignId;
            int oldTrainingId = 0;

            var existingMapping = await TenantDbCtx.TrainingCampaignMapping
               .Where(tcm => tcm.CampaignId == campaignId)
               .FirstOrDefaultAsync();

            if (existingMapping != null)
            {
                oldTrainingId = existingMapping.TrainingId;
                existingMapping.TrainingId = (int)trainingId;
                await TenantDbCtx.SaveChangesAsync();
                //TenantDbCtx.Remove(existingMapping);
            }
            else
            {
                var newMapping = new TrainingCampaignMapping
                {
                    TrainingId = trainingId.Value,
                    CampaignId = campaignId
                };

                TenantDbCtx.Add(newMapping);
                await TenantDbCtx.SaveChangesAsync();
            }
            
            var UpdateNewTrainingData = TenantDbCtx.Training.Where(o => o.Id == trainingId).FirstOrDefault();
            if (UpdateNewTrainingData != null)
            {
                UpdateNewTrainingData.TrainingTrigger = true;
                //await TenantDbCtx.SaveChangesAsync();

                var UpdateOldTrainingData = TenantDbCtx.TrainingCampaignMapping
               .Where(tcm => tcm.TrainingId == oldTrainingId)
               .ToList();
                if (UpdateOldTrainingData.Count == 0)
                {
                    var Check = TenantDbCtx.Training.Where(o => o.Id == oldTrainingId).FirstOrDefault();
                    if (Check != null)
                    {
                        Check.TrainingTrigger = false;
                    }
                }
                await TenantDbCtx.SaveChangesAsync();
            }
            return true;
        }


        public async Task<TrainingCampaignMapping> GetTrainingByCampaignId(int id)
        {
            try
            {
                var result = await TenantDbCtx.TrainingCampaignMapping
                    .Where(o => o.CampaignId == id)
                    .FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in GetTrainingByCampaignId for id: {id}");
                throw; // Rethrow the exception to propagate it
            }
        }



        public async Task<List<TrainingCampaignMapping>> GetTrainingCampaignsId(int id)
        {
            var compaignIds = TenantDbCtx.TrainingCampaignMapping.Where(o => o.TrainingId == id);
            return compaignIds.ToList();
        }

        public Task<IEnumerable<TrainingVideo>> GetTrainingVideos()
        {
            var result = Enumerable.Empty<TrainingVideo>();
            result = TenantDbCtx.TrainingVideoPath.OrderByDescending(o => o.Id);
            return Task.FromResult(result);
        }
        public async Task<TrainingVideo> UpsertTrainingVideo(TrainingVideo trainingVideo)
        {
            try
            {
                TenantDbCtx.Add(trainingVideo);
                TenantDbCtx.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }
            return await Task.FromResult(trainingVideo);
        }

        public async Task<List<Training>> GetAllTraining()
        {

            int currentYear = DateTime.Now.Year;
            int previousYear = currentYear - 1;
            var result = Enumerable.Empty<Training>();
            //result = TenantDbCtx.Campaigns.Where(o => o.CreatedOn.Year == currentYear || o.CreatedOn.Year == previousYear).OrderByDescending(o => o.Id);
            result = TenantDbCtx.Training.OrderByDescending(o => o.Id).ToList();
            return (List<Training>)result;

            //return TenantDbCtx.Training.ToList();
        }

        public async Task<List<TrainingQuizQuestion>> UpsertTrainingQuiz(List<TrainingQuizQuestion> dtos)
        {
            if (dtos == null || !dtos.Any())
            {
                throw new ArgumentNullException(nameof(dtos), "Invalid TrainingQuiz data");
            }
            var results = dtos;
            var existingQuestions = TenantDbCtx.TrainingQuizQuestion.Include(tq => tq.TrainingQuizAnswer)
              .Where(tq => tq.TrainingQuizId == dtos.First().TrainingQuizId).ToList();
            foreach (var existingQuestion in existingQuestions)
            {
                if (!dtos.Any(dto => dto.Id == existingQuestion.Id))
                {
                    TenantDbCtx.TrainingQuizQuestion.Remove(existingQuestion);
                }
            }
            foreach (var dto in dtos)
            {
                if (dto.Id > 0)
                {
                    var existingQuiz = await TenantDbCtx.TrainingQuizQuestion.Include(tq => tq.TrainingQuizAnswer)
                                            .FirstOrDefaultAsync(tq => tq.Id == dto.Id);
                    if (existingQuiz == null)
                    {
                        throw new ArgumentException($"No TrainingQuiz found with ID {dto.Id}", nameof(dtos));
                    }

                    existingQuiz.TrainingQuizId = dto.TrainingQuizId;
                    existingQuiz.Question = dto.Question;
                    existingQuiz.AnswerType = dto.AnswerType;
                    existingQuiz.OrderNumber = dto.OrderNumber;
                    existingQuiz.Weightage = dto.Weightage;
                    existingQuiz.IsActive = dto.IsActive;

                    foreach (var answer in existingQuiz.TrainingQuizAnswer.ToList())
                    {
                        if (!dto.TrainingQuizAnswer.Any(a => a.Id == answer.Id))
                        {
                            TenantDbCtx.TrainingQuizAnswer.Remove(answer);
                        }
                    }

                    foreach (var answerDto in dto.TrainingQuizAnswer)
                    {
                        var existingAnswer = existingQuiz.TrainingQuizAnswer
                                                .FirstOrDefault(a => a.Id == answerDto.Id);
                        if (existingAnswer == null)
                        {
                            var newAnswer = new TrainingQuizAnswer
                            {
                                TrainingQuizQuestionId = existingQuiz.Id,
                                AnswerText = answerDto.AnswerText,
                                OrderNumber = answerDto.OrderNumber,
                                IsCorrect = answerDto.IsCorrect
                            };
                            TenantDbCtx.TrainingQuizAnswer.Add(newAnswer);
                        }
                        else
                        {
                            existingAnswer.AnswerText = answerDto.AnswerText;
                            existingAnswer.OrderNumber = answerDto.OrderNumber;
                            existingAnswer.IsCorrect = answerDto.IsCorrect;
                        }
                    }
                }
                else
                {
                    var newQuiz = new TrainingQuizQuestion
                    {
                        TrainingQuizId = dto.TrainingQuizId,
                        Question = dto.Question,
                        AnswerType = dto.AnswerType,
                        OrderNumber = dto.OrderNumber,
                        Weightage = dto.Weightage,
                        IsActive = dto.IsActive
                    };
                    TenantDbCtx.TrainingQuizQuestion.Add(newQuiz);
                    await TenantDbCtx.SaveChangesAsync();

                    foreach (var answerDto in dto.TrainingQuizAnswer)
                    {
                        var newAnswer = new TrainingQuizAnswer
                        {
                            TrainingQuizQuestionId = newQuiz.Id,
                            AnswerText = answerDto.AnswerText,
                            OrderNumber = answerDto.OrderNumber,
                            IsCorrect = answerDto.IsCorrect
                        };
                        TenantDbCtx.TrainingQuizAnswer.Add(newAnswer);
                    }
                }
                try
                {
                    await TenantDbCtx.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return results;
        }


        public async Task<TrainingQuizResult> GetTrainingQuizById(int id)
        {
            TrainingQuizResult result;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var quiz = TenantDbCtx.TrainingQuiz.FirstOrDefault(i => i.Id == id);
            var questions = TenantDbCtx.TrainingQuizQuestion.Where(o => o.TrainingQuizId == id).Include(o => o.TrainingQuizAnswer).OrderBy(o => o.OrderNumber);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            result = new TrainingQuizResult
            {
                Quiz = quiz,
                Questions = questions
            };

            return result;
        }

        public async Task<IEnumerable<TrainingQuizQuestion>> GetQuizByTrainingId(int trainingId)
        {
            return TenantDbCtx.TrainingQuizQuestion.Where(o => o.TrainingQuizId == trainingId && o.IsActive)
                .Include(o => o.TrainingQuizAnswer).OrderBy(o => o.OrderNumber);
        }

        public async Task<List<TrainingQuiz>> UpsertTrainingQuizTitle(List<TrainingQuiz> data)
        {
            List<TrainingQuiz> result = null;

            if (data.Count() == 0)
                return result;

            foreach (TrainingQuiz tq in data)
            {
                var existing = await TenantDbCtx.TrainingQuiz.FirstOrDefaultAsync(c => c.Name.ToUpper() == tq.Name.ToUpper());
                if (existing != null)
                {
                    continue;
                }
                TenantDbCtx.TrainingQuiz.Add(tq);
            }
            TenantDbCtx.SaveChanges();

            return data;
        }

        public async Task<IEnumerable<TrainingQuiz>> GetAllTrainingQuiz()
        {
            IEnumerable<TrainingQuiz> result = null;

            result = TenantDbCtx.TrainingQuiz.ToList();
            return result;
        }

        public async Task<ApiResponse<TrainingQuizQuestion>> DeleteTrainingQuizQuestion(int id)
        {
            var result = new ApiResponse<TrainingQuizQuestion>();
            try
            {
                var existing = TenantDbCtx.TrainingQuizQuestion.First(x => x.Id == id);
                result.Result = existing;
                if (existing == null)
                {
                    result.IsSuccess = true;
                    result.Message = "Produc tList not found!";
                    return result;
                }

                TenantDbCtx.TrainingQuizQuestion.Remove(existing);
                await TenantDbCtx.SaveChangesAsync();
                result.IsSuccess = true;
                result.Message = "Successfully Deleted!";
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                return result;
            }
        }

        public async Task<ApiResponse<string>> CampaignSpamReport(string key)
        {
            var result = new ApiResponse<string>();
            var status = CampaignLogStatus.Sent.ToString();
            var campaignLog = TenantDbCtx.CampaignLogs
                .FirstOrDefault(o => o.SecurityStamp == key
                                    && o.IsReported == false);

            if (campaignLog == null)
            {
                campaignLog = TenantDbCtx.CampaignLogs
               .FirstOrDefault(o => o.SecurityStamp == key
                                   && o.IsReported == true);
                result.IsSuccess = false;
                result.Message = campaignLog == null ? "Invalid Url" : "You have already reported this mail.";
                return result;
            }



            var campaign = TenantDbCtx.Campaigns.FirstOrDefault(o => o.Id == campaignLog.CampaignId);
            if (campaign == null)
            {
                result.IsSuccess = false;
                result.Message = "Invalid Campaign";
                return result;
            }

            campaignLog.Status = CampaignLogStatus.Completed.ToString();
            campaignLog.IsReported = true;
            campaignLog.ModifiedOn = DateTime.Now;
            campaignLog.ModifiedBy = nameof(CampaignSpamReport);

            TenantDbCtx.Update(campaignLog);
            TenantDbCtx.SaveChanges();
            result.IsSuccess = true;
            return result;

        }

        #region Report
        public async Task<IEnumerable<CampaignLog>> BarChartDrillDownReportCount(int campId)
        { 
             var data = TenantDbCtx.CampaignLogs.Where(o => o.CampaignId == campId);
             return data;
        }

        public async Task<IQueryable<ReportDataCounts>> PieChartDrillDownReportCount(DrillDownReportCountParameter parameters)
        {
            var allIds = parameters.Ids.Split('a').Select(x => int.Parse(x)).ToList();

            var data = await TenantDbCtx.CampaignLogs
                .Include(o => o.Camp)
                .ThenInclude(o => o.Detail)
                .Include(o => o.Recipient)
                .ThenInclude(o => o.Recipient)
                .Where(x => allIds.Contains(x.CampaignId))
                .ToListAsync();

            var campaignLogs = data.AsQueryable();

            if (parameters.type == "Department")
            {
                campaignLogs = campaignLogs.Where(o => o.Recipient.Recipient.Department.Equals(parameters.filter, StringComparison.OrdinalIgnoreCase));
            }
            else if (parameters.type == "Category")
            {
                campaignLogs = campaignLogs.Where(o => o.Camp.Category.Equals(parameters.filter, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                campaignLogs = campaignLogs.Where(o => o.Camp.Detail.CampaignTemplateId == int.Parse(parameters.filter));
            }

            var filterData = campaignLogs.GroupBy(i => i.CampaignId)
                .Select(group => new ReportDataCounts
                {
                    CampaignId = group.Key,
                    CampaignName = string.Join(", ", group.Where(o => o.CampaignId == group.Key).Select(o => o.Camp.Name).Distinct()),
                    Total = group.Count(),
                    Hits = group.Count(g => g.IsHit),
                    Reported = group.Count(g => g.IsReported),
                });

            return filterData.AsQueryable();
        }

        #region UserDashBoard
        public async Task<TrainingStatics> GetUserDashBoardStats(string user)
        {
            var outcome = new TrainingStatics();
            int recipantId = TenantDbCtx.Recipients.Where(x=>x.Email==user).Select(x=>x.Id).FirstOrDefault();

            /*var lastTraining = TenantDbCtx.Training
                .OrderByDescending(c => c.ModifiedOn)
                .FirstOrDefault();*/

           //if (lastTraining != null)
            {
                /*var logs = TenantDbCtx.TrainingLog.Where(o => o.TrainingID == lastTraining.Id
                              && o.ReicipientID == recipantId
                              && (o.Status == TrainingLogStatus.Sent.ToString() || o.Status == TrainingLogStatus.Completed.ToString() || o.Status == TrainingLogStatus.InProgress.ToString()));*/

                var logs = TenantDbCtx.TrainingLog.Where(o => o.ReicipientID == recipantId);
                             

                outcome.TotalTrainingAssign = logs.Count();
                outcome.TrainingCompleted = logs.Count(o => o.Status == TrainingLogStatus.Completed.ToString());
                outcome.TrainingInprogess = logs.Count(o => o.Status == TrainingLogStatus.InProgress.ToString());
                outcome.TrainingNotAttampt = logs.Count(o => o.Status == TrainingLogStatus.Sent.ToString());

                if (outcome.TotalTrainingAssign > 0)
                {
                    outcome.TrainingCompromised = Math.Round(((decimal)outcome.TrainingCompleted / outcome.TotalTrainingAssign) * 100, 2);
                }
            }

            return await Task.FromResult(outcome);
        }

        public async Task<MonthlyTrainingBarChart> GetMonthwiseUserTrainingData(string user, int year)
        {
            MonthlyTrainingBarChart data = new MonthlyTrainingBarChart();
            data.MonthwiseTrainingEntry = new List<MonthlyTrainingReportData>();
            data.Year = year;
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31).AddHours(24).AddSeconds(-1);
            int recipantId = TenantDbCtx.Recipients.Where(x => x.Email == user).Select(x => x.Id).FirstOrDefault();

            try
            {
                var Traininglogs = TenantDbCtx.TrainingLog
                 .Where(i => i.ReicipientID == recipantId && i.SentOn >= start && i.SentOn < end);

                #region
                //var trainingGroup = Trainings.ToList().GroupBy(i => i.TrainingID, (key, entries) => new
                //{
                //    TrainingID = key,
                //    TotalTraining = entries.Count(),
                //    Completed = entries.Where(i => i.Equals(TrainingStatus.Completed)).Count(),
                //    Inprogress = entries.Where(i => i.Equals(TrainingStatus.InProgress)).Count()
                //});
                //foreach (var c in trainingGroup)
                //{
                //    var training = TenantDbCtx.Training.Find(c.TrainingID);

                //    if (training == null)
                //        continue;

                //    var entry = new TrainingCountsEntry()
                //    {
                //        Training = training,
                //        TotalTrainingAssign = c.TotalTraining,
                //        TrainingCompleted = c.Completed,
                //        TrainingInprogess = c.Inprogress,
                //    };

                //    TRData.TrainingCountEntries.Add(entry);
                //}
                //TRData.TrainingCountEntries = TRData.TrainingCountEntries.OrderByDescending(o => o.Training.ModifiedOn).Take(5).ToList();
                #endregion


                var trainingGroup = Traininglogs.ToList().GroupBy(i => i.SentOn.Month, (key, entries) => new
                {
                    Month = (Months)key,
                    TotalTraining = entries.Count(),
                    Completed = entries.Count(i => i.Status == (TrainingLogStatus.Completed).ToString()),
                    Inprogress = entries.Count(i => i.Status == (TrainingLogStatus.Pending).ToString()),

                });


                foreach (Months month in Enum.GetValues(typeof(Months)))
                {

                    var log = trainingGroup.FirstOrDefault(i => i.Month == month);

                    var entry = new MonthlyTrainingReportData
                    {
                        Month = month,
                    };

                    if (log != null)
                    {
                        entry.TotalTraining = log.TotalTraining;
                        entry.Completed = log.Completed;
                        entry.Inprogress = log.Inprogress;
                        if (entry.TotalTraining > 0)
                        {
                            entry.CompletionPercent = Math.Round(((decimal)entry.Completed / entry.TotalTraining) * 100, 2);
                        }

                    }


                    data.MonthwiseTrainingEntry.Add(entry);
                }




            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
            }
            return await Task.FromResult(data);
        }


        #endregion

        #endregion
    }
}
