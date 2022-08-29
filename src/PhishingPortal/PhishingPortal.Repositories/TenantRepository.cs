using Microsoft.Extensions.Logging;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Dto.Dashboard;

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

            result = TenantDbCtx.Campaigns.Include(o => o.Schedule).OrderByDescending(o => o.Id)
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

            TenantDbCtx.SaveChanges();

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
                        .FirstOrDefault(o => o.Email == r.Email || o.Mobile == r.Mobile);

                    recipient.Mobile = r.Mobile;
                    recipient.Name = r.Name;
                    recipient.Email = r.Email;

                    TenantDbCtx.Update(recipient);
                    hasChanges = true;
                    if (!TenantDbCtx.CampaignRecipients.Any(o => o.CampaignId == campaignId && o.RecipientId == recipient.Id))
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

        /// <summary>
        /// Phsihing prone percentage - group by phishing category
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<CategoryWisePhishingTestData> GetCategoryWisePhishingReport(DateTime start, DateTime end)
        {
            CategoryWisePhishingTestData data = new CategoryWisePhishingTestData();

            data.Entries = new List<PhisingPronePercentEntry>();
            data.CategoryClickRatioDictionary = new Dictionary<string, decimal>();
            data.DepartEntries = new Dictionary<string, decimal>();
            data.TemplateClickEntries = new Dictionary<string, decimal>();

            try
            {

                var totatPhishingTests = TenantDbCtx.CampaignLogs
                  .Where(i => i.CreatedOn >= start && i.CreatedOn < end);


                var phishtestWithRecipients = from log in totatPhishingTests
                                              join crec in TenantDbCtx.CampaignRecipients.Include(o => o.Recipient) on log.RecipientId equals crec.RecipientId
                                              select new { logEntry = log, Department = crec.Recipient.Department };
                var depatwiseCnt = phishtestWithRecipients.ToList().GroupBy(i => i.Department, (key, entries) => new
                {
                    Department = key ?? "UNKNOWN",
                    Total = entries.Count(),
                    Hits = entries.Count(o => o.logEntry.IsHit)
                });
                var DtotalHits = depatwiseCnt.Sum(o => o.Hits);

                foreach (var dep in depatwiseCnt)
                {
                    if (dep.Total > 0)
                    {
                        var equivDep = CalcEquivalentPercent(dep.Hits, DtotalHits);

                        if (!data.DepartEntries.ContainsKey(dep.Department))
                        {
                            data.DepartEntries.Add(dep.Department, equivDep);
                        }


                    }
                }

                var phishtestWithTemp = from log in totatPhishingTests
                                        join cdet in TenantDbCtx.CampaignDetails on log.CampaignId equals cdet.CampaignId
                                        join ctem in TenantDbCtx.CampaignTemplates on cdet.CampaignTemplateId equals ctem.Id
                                        select new { logEntry = log, template = ctem.Name };

                var tempwiseCnt = phishtestWithTemp.ToList().GroupBy(i => i.template, (key, tentries) => new
                {
                    template = key,
                    TTotal = tentries.Count(),
                    THits = tentries.Count(o => o.logEntry.IsHit)
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

                var campaignGroup = totatPhishingTests.ToList().GroupBy(i => i.CampaignId, (key, entries) => new
                {
                    CampaignId = key,
                    Total = entries.Count(),
                    TotalHits = entries.Count(i => i.IsHit),
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
                    };

                    data.Entries.Add(entry);
                }
                data.Entries = data.Entries.OrderByDescending(o => o.Campaign.ModifiedOn).Take(5).ToList();

                data.TotalCampaigns = data.Entries.Sum(i => i.Count);


                // create category wise phish prone %
                var categoryWiseGrp = data.Entries.GroupBy(o => o.Campaign.Category, (key, values) => new
                {
                    Category = key,
                    Count = values.Sum(o => o.Count),
                    HitCount = values.Sum(o => o.Hits),
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
                if (month <= (Months)DateTime.Now.Month)
                {
                    entry.TotalCount = new Random().Next(50, 100);
                    entry.TotalHits = new Random().Next(15, 39);
                    entry.HitPronePercent = Math.Round(((decimal)entry.TotalHits / (decimal)entry.TotalCount) * 100, 2);
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
            var settings = await TenantDbCtx.Settings.ToListAsync();

            return settings.ToDictionary(o => o.Key, v => v.Value);
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
    }
}