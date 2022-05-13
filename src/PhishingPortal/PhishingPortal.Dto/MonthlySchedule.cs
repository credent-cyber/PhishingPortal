using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class MonthlySchedule : BaseSchedule
    {
        public int DayOfMonth { get; set; }
        public TimeOfDay ScheduleTime { get; set; }

        public MonthlySchedule(string value)
        {
            //var val = JsonConvert.DeserializeObject<MonthlySchedule>(value);
            DayOfMonth = int.Parse(value.Split("|")[0]);
            ScheduleTime = new TimeOfDay(value.Split("|")[1]);
        }

        public override bool Eval()
        {
            var day = DateTime.Now.Date.Day;

            if (day == DayOfMonth)
            {
                var timespan = this.ScheduleTime.ToTimeSpan();
                var minutesSoFar = (DateTime.Now - DateTime.Now.Date).TotalMinutes;
                return minutesSoFar >= timespan.TotalMinutes && minutesSoFar - timespan.TotalMinutes < 10;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{DayOfMonth}|{ScheduleTime}";
        }
    }
}
