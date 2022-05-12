using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class MonthlySchedule : BaseSchedule
    {
        public int DayOfMonth { get; set; }
        public TimeSpan ScheduleTime { get; set; }

        public MonthlySchedule(string value)
        {
            var val = JsonConvert.DeserializeObject<MonthlySchedule>(value);
            DayOfMonth = val.DayOfMonth;
            ScheduleTime = val.ScheduleTime;
        }

        public override bool Eval()
        {
            var day = DateTime.Now.Date.Day;

            if (day == DayOfMonth)
            {
                var minutesSoFar = (DateTime.Now - DateTime.Now.Date).TotalMinutes;
                return minutesSoFar >= ScheduleTime.TotalMinutes && minutesSoFar - ScheduleTime.TotalMinutes < 10;
            }

            return false;
        }
    }
}
