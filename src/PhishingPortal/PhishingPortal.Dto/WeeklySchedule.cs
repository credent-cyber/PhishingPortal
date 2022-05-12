using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class WeeklySchedule : BaseSchedule
    {
        public DayOfWeek WeekDay { get; set; }
        public TimeSpan ScheduleTime { get; set; }

        public WeeklySchedule(string value)
        {
            var weekDayStr = value.Split("|")[0];
            var timespan = value.Split("|")[1];

            this.WeekDay = Enum.Parse<DayOfWeek>(weekDayStr);
            this.ScheduleTime = TimeSpan.Parse(timespan);
        }

        public override bool Eval()
        {
            var weekDay = DateTime.Now.DayOfWeek;
            if (weekDay == WeekDay)
            {
                var minutesSoFar = (DateTime.Now - DateTime.Now.Date).TotalMinutes;
                return minutesSoFar >= ScheduleTime.TotalMinutes && minutesSoFar - ScheduleTime.TotalMinutes < 10;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{WeekDay.ToString()}|{ScheduleTime.ToString()}";
        }
    }
}
