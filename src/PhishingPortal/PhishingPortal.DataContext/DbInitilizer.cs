using PhishingPortal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.DataContext
{
    public static class DbInitilizer
    {

        public static void SeedDefaults(this TenantDbContext dbContext)
        {

            var metalist = new List<Dto.Metadata>
            {
                new Dto.Metadata
                {
                     Key = "Link1",
                     Value = "http://localhost:7081/campaign/landing/page-1"
                },
                new Dto.Metadata
                {
                     Key = "Banner1",
                     Value = "http://localhost:7081/img/banner.jpg"
                }
            };

            dbContext.MetaContents.AddRange(metalist);

            var template = new Dto.CampaignTemplate
            {
                IsActive = true,
                CreatedBy = "system",
                CreatedOn = DateTime.Now,
                IsHtml = true,
                Type = Dto.CampaignType.Email,
                Content = "<div><img alt='banner' src='###Banner1###' /><br /><p>Loreum epsum loreum epsum loreum epsum loreum epsum loreum epsum loreum epsum</p></div>"
            };

            dbContext.CampaignTemplates.Add(template);

            var template2 = new Dto.CampaignTemplate
            {
                IsActive = false,
                CreatedBy = "system",
                CreatedOn = DateTime.Now,
                IsHtml = true,
                Type = Dto.CampaignType.Email,
                Content = "<div><img alt='banner' src='###Banner1###' /><br /><p>Loreum epsum loreum epsum loreum epsum loreum epsum loreum epsum loreum epsum</p></div>"
            };

            dbContext.CampaignTemplates.Add(template2);

            var campaign = new Dto.Campaign
            {
                IsActive = true,
                Name = "Sample Email Campaign",
                Description = "Sample Campaign description",
                State = Dto.CampaignStateEnum.Draft,
                Detail = new Dto.CampaignDetail
                {
                    Template = template,
                    Type = Dto.CampaignType.Email,
                },
                Schedule = new Dto.CampaingSchedule
                {
                    ScheduleType = Dto.ScheduleTypeEnum.Once,
                    WillRepeat = false,
                    //OnceOffSchedule = new Dto.OnceOffSchedule()
                    //{
                    //     DateTime = DateTime.Now.Date.AddHours(12), // 12:00 noon
                    //}
                }
            };

            dbContext.Campaigns.Add(campaign);

            dbContext.SaveChanges();

        }
    }
}
