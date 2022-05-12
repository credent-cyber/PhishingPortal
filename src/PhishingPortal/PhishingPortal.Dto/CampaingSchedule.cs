using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class CampaingSchedule : BaseEntity
    {
        public string CampaignId { get; set; }
        public ScheduleTypeEnum ScheduleType { get; set; }
        public bool WillRepeat { get; set; }
        public string ScheduleInfo { get; set; }

        [NotMapped]
        public OnceOffSchedule OnceOffSchedule
        {
            get
            {
                if (ScheduleType == ScheduleTypeEnum.Once)
                    return new OnceOffSchedule(this.ScheduleInfo);

                return null;
            }
            set
            {
                if (value != null)
                    ScheduleInfo = value.ToString();
            }
        }

        [NotMapped]
        public DailySchedule DailySchedule
        {
            get
            {
                if (ScheduleType == ScheduleTypeEnum.Daily)
                    return new DailySchedule(this.ScheduleInfo);

                return null;
            }
            set
            {
                if (value != null)
                    ScheduleInfo = value.ToString();
            }
        }

        [NotMapped]
        public WeeklySchedule WeeklySchedule
        {
            get
            {
                if (ScheduleType == ScheduleTypeEnum.Weekly)
                    return new WeeklySchedule(this.ScheduleInfo);

                return null;
            }
            set
            {
                if (value != null)
                    ScheduleInfo = value.ToString();
            }
        }

        [NotMapped]
        public MonthlySchedule MonthlySchedule
        {
            get
            {
                if (ScheduleType == ScheduleTypeEnum.Monthly)
                    return new MonthlySchedule(this.ScheduleInfo);

                return null;
            }
            set
            {
                if (value != null)
                    ScheduleInfo = value.ToString();
            }
        }

       
    }
}
