using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class CampaignSchedule : BaseEntity
    {
        public ScheduleTypeEnum ScheduleType { get; set; }
        public bool WillRepeat { get; set; }
        public string ScheduleInfo { get; set; }
    }
}
