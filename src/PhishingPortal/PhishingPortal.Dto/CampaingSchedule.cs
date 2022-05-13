using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class CampaingSchedule : BaseEntity
    {
        public string CampaignId { get; set; }
        public ScheduleTypeEnum ScheduleType { get; set; }
        public bool WillRepeat { get; set; }
        public string ScheduleInfo { get; set; }
    }
}
